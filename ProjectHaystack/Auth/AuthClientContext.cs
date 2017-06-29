//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ProjectHaystack.Client;
using ProjectHaystack.Util;

namespace ProjectHaystack.Auth
{
  public sealed class AuthClientContext
  {
    //////////////////////////////////////////////////////////////////////////
    // Construction
    //////////////////////////////////////////////////////////////////////////

    public AuthClientContext(string uri, string user, string pass)
    {
      this.uri = uri;
      this.user = user;
      this.pass = pass;
    }

    //////////////////////////////////////////////////////////////////////////
    // State
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    ///   URI used to Open the connection
    /// </summary>
    public readonly string uri;

    /// <summary>
    ///   Username used to Open the connection
    /// </summary>
    public readonly string user;

    /// <summary>
    ///   Plaintext password for authentication
    /// </summary>
    public string pass;

    /// <summary>
    ///   User agent string
    /// </summary>
    public string userAgent = "HaystackC#";

    /// <summary>
    ///   Headers we wish to Use for authentication requests
    /// </summary>
    public IDictionary headers = new Hashtable();

    /// <summary>
    ///   Stash allows you to store state between messages
    ///   while authenticating with the server.
    /// </summary>
    public IDictionary stash = new Hashtable();

    /// <summary>
    ///   Have we Successfully authenticated to the server
    /// </summary>
    public bool Authenticated
    {
      get { return authenticated; }
    }

    //////////////////////////////////////////////////////////////////////////
    // Open
    //////////////////////////////////////////////////////////////////////////

    public AuthClientContext Open()
    {
      try
      {
        // send initial hello message
        var helloResp = SendHello();
        // first try standard authentication
        if (OpenStd(helloResp))
        {
          return Success();
        }
        // check if we have a 200
        if ((int) helloResp.StatusCode == 200)
        {
          return Success();
        }

        var content = ReadContent(helloResp);
        var schemes = AuthScheme.List();
        for (var i = 0; i < schemes.Length; ++i)
        {
          if (schemes[i].OnClientNonStd(this, helloResp, content))
          {
            return Success();
          }
        }

        // give up
        var resCode = (int) helloResp.StatusCode;
        var resServer = helloResp.GetResponseHeader("Server");
        if (resCode / 100 >= 4)
        {
          throw new IOException("HTTP error code: " + resCode); // 4xx or 5xx
        }
        throw new AuthException("No suitable auth scheme for: " + resCode + " " + resServer);
      }
      catch (AuthException e)
      {
        throw e;
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

    private HttpWebResponse SendHello()
    {
      // hello message
      try
      {
        var parameters = new SortedDictionary<string, string>();
        parameters["username"] = Base64.URI.EncodeUtf8(user);
        var hello = new AuthMsg("hello", parameters);
        return GetAuth(hello);
      }
      catch (WebException e)
      {
        var response = e.Response;
        var httpresp = (HttpWebResponse) response;
        if ((int) httpresp.StatusCode == 401)
        {
          return httpresp;
        }
        throw e;
      }
    }

    private AuthClientContext Success()
    {
      authenticated = true;
      return this;
    }

    /// <summary>
    ///   Attempt standard authentication
    /// </summary>
    /// <param name="resp"> The response to the hello message </param>
    /// <returns>
    ///   true if haystack authentciation was used, false if the
    ///   server does not appear to implement RFC 7235.
    /// </returns>
    private bool OpenStd(HttpWebResponse resp)
    {
      try
      {
        // must be 401 challenge with WWW-Authenticate header
        if ((int) resp.StatusCode != 401)
        {
          return false;
        }
        var wwwAuth = ResHeader(resp, "WWW-Authenticate");

        // don't Use this mechanism for Basic which we
        // handle as a non-standard scheme because the headers
        // don't fit nicely into our restricted AuthMsg format
        if (wwwAuth.ToLower().StartsWith("basic", StringComparison.Ordinal))
        {
          return false;
        }
        // process res/req messages until we have 200 or non-401 failure
        AuthScheme scheme = null;
        for (var loopCount = 0;; ++loopCount)
        {
          // sanity check that we don't loop too many times
          if (loopCount > 5)
          {
            throw new AuthException("Loop count exceeded");
          }

          // parse the WWW-Auth header and Use the first scheme
          var header = ResHeader(resp, "WWW-Authenticate");
          AuthMsg[] resMsgs = AuthMsg.ListFromStr(header);
          var resMsg = resMsgs[0];
          scheme = AuthScheme.Find(resMsg.scheme);

          // let scheme handle message
          var reqMsg = scheme.OnClient(this, resMsg);
          // send request back to the server
          resp = GetAuth(reqMsg);
          try
          {
            DumpRes(resp, false);
          }
          catch (Exception e)
          {
            e.ToString();
          }
          // 200 means we are done, 401 means keep looping,
          // consider anything else a failure
          if ((int) resp.StatusCode == 200)
          {
            break;
          }
          if ((int) resp.StatusCode == 401)
          {
            continue;
          }
          throw new AuthException((int) resp.StatusCode + " " + resp.GetResponseStream());
        }
        // init the bearer token
        var authInfo = ResHeader(resp, "Authentication-Info");
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

        // we did it!
        return true;
      }
      catch (IOException e)
      {
        throw e;
      }
    }

    ////////////////////////////////////////////////////////////////
    // HTTP Messaging
    ////////////////////////////////////////////////////////////////

    /// <summary>
    ///   Get a new http connection to the given uri.
    /// </summary>
    public HttpWebRequest OpenHttpConnection(string uri, string method)
    {
      try
      {
        return HClient.OpenHttpConnection(new Uri(uri), method, connectTimeout, readTimeout);
      }
      catch (IOException e)
      {
        throw e;
      }
    }

    public void AddCookiesToHeaders(HttpWebRequest c)
    {
      var cookies = (IList<string>) c.Headers;
      if (cookies == null || cookies.Count == 0)
      {
        return;
      }
      IEnumerator iter = cookies.GetEnumerator();
      var sb = new StringBuilder();
      var first = true;
      while (iter.MoveNext())
      {
        var cookie = (string) iter.Current;
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

    private HttpWebResponse GetAuth(AuthMsg msg)
    {
      try
      {
        // all AuthClientContext requests are GET message to the /About uri
        var c = Prepare(OpenHttpConnection(uri, "GET"));

        // set Authorization header
        c.Headers.Set("Authorization", msg.ToString());
        return Get(c);
      }
      catch (IOException e)
      {
        throw e;
      }
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
      var iter = headers.Keys.GetEnumerator();
      while (iter.MoveNext())
      {
        var key = (string) iter.Current;
        var val = (string) headers[key];
        c.Headers[key] = val;
      }
      if (userAgent != null)
      {
        c.UserAgent = userAgent;
      }
      return c;
    }

    private HttpWebResponse Get(HttpWebRequest c)
    {
      try
      {
        // connect and return response
        try
        {
          var resp = c.GetResponse();
          var httpresp = (HttpWebResponse) resp;
          return httpresp;
        }
        catch (WebException e)
        {
          var response = e.Response;
          var httpresp = (HttpWebResponse) response;
          if ((int) httpresp.StatusCode == 401)
          {
            return httpresp;
          }
          else
          {
            throw e;
          }
        }
        finally
        {
          try
          {
            c.Abort();
          }
          catch (Exception e)
          {
            throw e;
          }
        }
      }
      catch (IOException e)
      {
        throw e;
      }
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
        for (var it = (IEnumerator) c.Headers; it.MoveNext();)
        {
          var key = (string) it.Current;
          var val = c.GetResponseHeader(key);
          Console.WriteLine(key + ": " + val);
        }
        Console.WriteLine();
        if (body)
        {
          var @in = c.GetResponseStream();
          int n;
          while ((n = @in.ReadByte()) > 0)
          {
            Console.Write((char) n);
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