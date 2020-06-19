//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ProjectHaystack.Util;

namespace ProjectHaystack.Auth
{
  public sealed class AsyncAuthClientContext : IAuthClientContext
  {
    //////////////////////////////////////////////////////////////////////////
    // Construction
    //////////////////////////////////////////////////////////////////////////

    public AsyncAuthClientContext(Uri uri, string user, string pass)
    {
      if (user.Length == 0)
      {
        throw new ArgumentException("user cannot be empty string");
      }

      Uri = uri.EndWithSlash();
      this.user = user;
      this.pass = pass;
      ServerCallAsync = ServerCallAsyncDefault;
    }

    //////////////////////////////////////////////////////////////////////////
    // State
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///   URI used to Open the connection
    /// </summary>
    public string uri { get { return Uri.AbsoluteUri; } }
    public Uri Uri { get; private set; }

    /// <summary>
    ///   Username used to Open the connection
    /// </summary>
    public string user { get; private set; }

    /// <summary>
    ///   Plaintext password for authentication
    /// </summary>
    public string pass { get; private set; }

    /// <summary>
    ///   User agent string
    /// </summary>
    public string userAgent = "HaystackC#";

    /// <summary>
    ///   Headers we wish to Use for authentication requests
    /// </summary>
    public IDictionary headers { get; private set; } = new Hashtable();

    /// <summary>
    ///   Stash allows you to store state between messages
    ///   while authenticating with the server.
    /// </summary>
    public IDictionary stash { get; private set; } = new Hashtable();

    /// <summary>
    ///   Have we Successfully authenticated to the server
    /// </summary>
    public bool Authenticated
    {
      get { return authenticated; }
    }

    public ServerCallAsync ServerCallAsync { get; set; }

    public ServerCall ServerCall => throw new NotImplementedException();

    //////////////////////////////////////////////////////////////////////////
    // Open
    //////////////////////////////////////////////////////////////////////////

    public async Task OpenAsync()
    {
      try
      {
        // send initial hello message
        using (var helloResp = await SendHelloAsync())
        {
          // first try standard authentication
          if (await OpenStdAsync(helloResp))
          {
            authenticated = true;
            return;
          }
          // check if we have a 200
          if (helloResp.StatusCode == HttpStatusCode.OK)
          {
            authenticated = true;
            return;
          }

          var content = ReadContent(helloResp);
          var schemes = AuthScheme.List();
          for (var i = 0; i < schemes.Length; ++i)
          {
            if (await schemes[i].OnClientNonStdAsync(this, helloResp, content))
            {
              authenticated = true;
              return;
            }
          }

          // give up
          var resCode = (int)helloResp.StatusCode;
          var resServer = helloResp.GetResponseHeader("Server");
          if (resCode / 100 >= 4)
          {
            throw new IOException("HTTP error code: " + resCode); // 4xx or 5xx
          }
          throw new AuthException("No suitable auth scheme for: " + resCode + " " + resServer);
        }
      }
      catch (AuthException)
      {
        throw;
      }
      catch (Exception e)
      {
        throw new AuthException("authenticate failed", e);
      }
      finally
      {
        pass = null;
        stash.Clear();
      }
    }

    private async Task<HttpWebResponse> SendHelloAsync()
    {
      // hello message
      try
      {
        var parameters = new SortedDictionary<string, string>();
        parameters["username"] = Base64.URI.EncodeUtf8(user);
        var hello = new AuthMsg("hello", parameters);
        return await GetAuthAsync(hello);
      }
      catch (WebException e)
      {
        var response = e.Response;
        if (response != null)
        {
          var httpresp = (HttpWebResponse)response;
          // 401 Unauthorized
          // 500 Internal Server Error, for compatibility with nhaystack
          if (httpresp.StatusCode == HttpStatusCode.Unauthorized || httpresp.StatusCode == HttpStatusCode.InternalServerError)
          {
            return httpresp;
          }
        }
        throw;
      }
    }

    /// <summary>
    ///   Attempt standard authentication
    /// </summary>
    /// <param name="resp"> The response to the hello message </param>
    /// <returns>
    ///   true if haystack authentciation was used, false if the
    ///   server does not appear to implement RFC 7235.
    /// </returns>
    private async Task<bool> OpenStdAsync(HttpWebResponse resp)
    {
      var innerResponse = resp;
      try
      {
        // must be 401 challenge with WWW-Authenticate header
        if (resp.StatusCode != HttpStatusCode.Unauthorized)
        {
          return false;
        }
        var wwwAuth = ResHeader(resp, "WWW-Authenticate");

        // don't Use this mechanism for Basic which we
        // handle as a non-standard scheme because the headers
        // don't fit nicely into our restricted AuthMsg format
        if (string.IsNullOrEmpty(wwwAuth) || wwwAuth.ToLower().StartsWith("basic", StringComparison.Ordinal))
        {
          return false;
        }
        // process res/req messages until we have 200 or non-401 failure
        AuthScheme scheme = null;
        for (var loopCount = 0; ; ++loopCount)
        {
          // sanity check that we don't loop too many times
          if (loopCount > 5)
          {
            throw new AuthException("Loop count exceeded");
          }

          // parse the WWW-Auth header and Use the first scheme
          var header = ResHeader(innerResponse, "WWW-Authenticate");
          AuthMsg[] resMsgs = AuthMsg.ListFromStr(header);
          var resMsg = resMsgs[0];
          scheme = AuthScheme.Find(resMsg.scheme);

          // Dispose the previous response.
          if (innerResponse != resp)
            innerResponse.Dispose();

          // let scheme handle message
          var reqMsg = scheme.OnClient(this, resMsg);
          // send request back to the server
          innerResponse = await GetAuthAsync(reqMsg);
          try
          {
            DumpRes(innerResponse, false);
          }
          catch (Exception e)
          {
            e.ToString();
          }
          // 200 means we are done, 401 means keep looping,
          // consider anything else a failure
          if (innerResponse.StatusCode == HttpStatusCode.OK)
          {
            break;
          }
          if (innerResponse.StatusCode == HttpStatusCode.Unauthorized)
          {
            continue;
          }
          throw new AuthException((int)innerResponse.StatusCode + " " + innerResponse.GetResponseStream());
        }
        // init the bearer token
        var authInfo = ResHeader(innerResponse, "Authentication-Info");
        AuthMsg authInfoMsg = AuthMsg.FromStr("bearer " + authInfo);

        // callback to scheme for client Success
        scheme.OnClientSuccess(this, authInfoMsg);

        // only keep authToken parameter for Authorization header
        authInfoMsg = new AuthMsg("bearer", new[]
        {
          "authToken",
          authInfoMsg.Param("authToken")
        });
        headers["Authorization"] = authInfoMsg.ToString();
      }
      finally
      {
        // Dispose the last response.
        if (innerResponse != resp)
          resp.Dispose();
      }

      // we did it!
      return true;
    }

    ////////////////////////////////////////////////////////////////
    // HTTP Messaging
    ////////////////////////////////////////////////////////////////

    public void AddCookiesToHeaders(HttpWebRequest c)
    {
      var cookies = c.Headers;
      if (cookies == null || cookies.Count == 0)
      {
        return;
      }
      IEnumerator iter = cookies.GetEnumerator();
      var sb = new StringBuilder();
      var first = true;
      while (iter.MoveNext())
      {
        var cookie = (string)iter.Current;
        var semi = cookie.IndexOf(";", StringComparison.Ordinal);
        if (semi <= 0)
        {
          continue;
        }

        if (first)
        {
          first = false;
        }
        else
        {
          sb.Append(";");
        }

        // add name=value pair
        sb.Append(cookie.Substring(0, semi));
      }
      headers["Cookie"] = sb.ToString();
    }

    private async Task<HttpWebResponse> GetAuthAsync(AuthMsg msg)
    {
      return await ServerCallAsync("about", c =>
      {
        // set Authorization header
        c.Headers.Set("Authorization", msg.ToString());
      });
    }

    /// <summary>
    /// Prepares a httpwebrequest instance with the auth cookies/headers
    /// </summary>
    /// <param name="c">the httpwebrequest to configure</param>
    /// <returns>the configured httpwebrequest</returns>
    public HttpWebRequest Prepare(HttpWebRequest c)
    {
      // set headers
      if (headers == null)
      {
        headers = new Hashtable();
      }
      foreach (string key in headers.Keys)
      {
        var val = (string)headers[key];
        c.Headers[key] = val;
      }
      if (userAgent != null)
      {
        c.UserAgent = userAgent;
      }
      return c;
    }

    private string ReadContent(HttpWebResponse resp)
    {
      try
      {
        // If there is non content-type header, then assume no content.
        if (resp.GetResponseHeader("Content-Type") == null)
        {
          return null;
        }
        try
        {
          // check for error stream first; if null, then get standard input stream
          var sr = resp.GetResponseStream();
          // read content
          var sb = new StringBuilder();
          try
          {
            sr = new BufferedStream(sr);
            var br = new StreamReader(sr);
            string line = null;
            while ((line = br.ReadLine()) != null)
            {
              sb.Append(line);
            }
            return sb.ToString();
          }
          finally
          {
            if (sr != null)
            {
              try
              {
                sr.Close();
              }
              catch (Exception)
              {
              }
            }
          }
        }
        catch (IOException e)
        {
          Console.WriteLine(e.ToString());
          Console.Write(e.StackTrace);
          return null;
        }
      }
      catch (IOException e)
      {
        throw e;
      }
    }

    private async Task<HttpWebResponse> ServerCallAsyncDefault(string action, Action<HttpWebRequest> requestConfigurator)
    {
      HttpWebRequest c = (HttpWebRequest)WebRequest.Create(new Uri(Uri, action));
      try
      {
        c.Method = "GET";
        c.AllowAutoRedirect = false;
        c.Timeout = connectTimeout;
        c.ReadWriteTimeout = readTimeout;
        c = Prepare(c);
        requestConfigurator(c);
        var response = (HttpWebResponse)(await c.GetResponseAsync());
        if ((int)response.StatusCode >= 300 || (int)response.StatusCode < 200)
        {
          response.Dispose();
          throw new WebException("Invalid status code", null, WebExceptionStatus.ProtocolError, response);
        }
        return response;
      }
      catch (WebException e)
      {
        var httpresp = (HttpWebResponse)e.Response;
        if (httpresp?.StatusCode == HttpStatusCode.Unauthorized)
        {
          return httpresp;
        }
        throw;
      }
      catch (Exception)
      {
        c.Abort();
        throw;
      }
    }

    private string ResHeader(HttpWebResponse c, string name)
    {
      var val = c.GetResponseHeader(name);
      if (val == null)
      {
        throw new AuthException("Missing required header: " + name);
      }
      return val;
    }

    //////////////////////////////////////////////////////////////////////////
    // Debug Utils
    //////////////////////////////////////////////////////////////////////////

    private void DumpRes(HttpWebResponse c, bool body)
    {
      try
      {
        Console.WriteLine("====  " + c.ResponseUri);
        Console.WriteLine("res: " + c.StatusCode + " " + c.GetResponseStream());
        foreach (var key in c.Headers.AllKeys)
        {
          var val = c.Headers[key];
          Console.WriteLine(key + ": " + val);
        }
        Console.WriteLine();
        if (body)
        {
          var @in = c.GetResponseStream();
          int n;
          while ((n = @in.ReadByte()) > 0)
          {
            Console.Write((char)n);
          }
        }
      }
      catch (IOException e)
      {
        throw e;
      }
    }

    //////////////////////////////////////////////////////////////////////////
    // Fields
    //////////////////////////////////////////////////////////////////////////


    /// <summary>
    ///   Set true after Successful authentication
    /// </summary>
    private bool authenticated;

    /// <summary>
    ///   Timeout in milliseconds for opening the HTTP socket
    /// </summary>
    public int connectTimeout = 60 * 1000;

    /// <summary>
    ///   Timeout in milliseconds for reading from the HTTP socket
    /// </summary>
    public int readTimeout = 60 * 1000;
  }
}