//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Web;
using System.Collections;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace ProjectHaystack.Client
{
  using AuthClientContext = ProjectHaystack.Auth.AuthClientContext;
  using AuthMsg = ProjectHaystack.Auth.AuthMsg;
  using Base64 = ProjectHaystack.Util.Base64;
  using Pbkdf2 = ProjectHaystack.Util.Pbkdf2;
  using Scramscheme = ProjectHaystack.Auth.ScramScheme;

  /// <summary>
  /// HClient manages a logical connection to a HTTP REST haystack server.
  /// </summary>
  /// <seealso> cref= <a href='http://project-haystack.org/doc/Rest'>Project Haystack</a> </seealso>
  public class HClient
  {

    //////////////////////////////////////////////////////////////////////////
    // Construction
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Convenience for construction and call to Open().
    /// </summary>
    public static HClient Open(string uri, string user, string pass)
    {
      return (new HClient(uri, user, pass)).Open();
    }

    /// <summary>
    /// Convenience for constructing client with custom timeouts and call to Open()
    /// </summary>
    public static HClient Open(string uri, string user, string pass, int connectTimeout, int readTimeout)
    {
      return (new HClient(uri, user, pass)).SetTimeouts(connectTimeout, readTimeout).Open();
    }

    /// <summary>
    /// Constructor with URI to server's API and authentication credentials.
    /// </summary>
    public HClient(string uri, string user, string pass)
    {
      // check uri
      if (!uri.StartsWith("http://", StringComparison.Ordinal) && !uri.StartsWith("https://", StringComparison.Ordinal))
      {
        throw new System.ArgumentException("Invalid uri format: " + uri);
      }
      if (!uri.EndsWith("/", StringComparison.Ordinal))
      {
        uri = uri + "/";
      }

      // sanity check arguments
      if (user.Length == 0)
      {
        throw new System.ArgumentException("user cannot be empty string");
      }

      this.uri = uri;
      this.auth = new AuthClientContext(uri + "about", user, pass);
    }

    //////////////////////////////////////////////////////////////////////////
    // State
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Base URI for connection such as "http://host/api/demo/".
    ///    This string always ends with slash. 
    /// </summary>
    public readonly string uri;

    /// <summary>
    /// Timeout in milliseconds for opening the HTTP socket </summary>
    public int connectTimeout = 60 * 1000;

    /// <summary>
    /// Set the connect timeout and return this </summary>
    public virtual HClient SetConnectTimeout(int timeout)
    {
      if (timeout < 0)
      {
        throw new System.ArgumentException("Invalid timeout: " + timeout);
      }
      this.connectTimeout = timeout;
      return this;
    }

    /// <summary>
    /// Timeout in milliseconds for reading from the HTTP socket </summary>
    public int readTimeout = 60 * 1000;

    /// <summary>
    /// Set the read timeout and return this </summary>
    public virtual HClient SetReadTimeout(int timeout)
    {
      if (timeout < 0)
      {
        throw new System.ArgumentException("Invalid timeout: " + timeout);
      }
      this.readTimeout = timeout;
      return this;
    }

    /// <summary>
    /// Set the connect and read timeouts and return this </summary>
    public virtual HClient SetTimeouts(int connectTimeout, int readTimeout)
    {
      return SetConnectTimeout(connectTimeout).SetReadTimeout(readTimeout);
    }

    //////////////////////////////////////////////////////////////////////////
    // Operations
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Authenticate the client and return this.
    /// </summary>
    public virtual HClient Open()
    {
      auth.connectTimeout = this.connectTimeout;
      auth.readTimeout = this.readTimeout;
      auth.Open();
      return this;
    }

    /// <summary>
    /// Gets the raw string from request passed in
    /// </summary>
    /// <param name="op">Given operation</param>
    /// <param name="params">Dictionary containing search parameters</param>
    /// <param name="mimeRequest">Mime type for ContentType header</param>
    /// <param name="mimeResponse">Mime type for Accept header</param>
    /// <returns>Raw string of the result</returns>
    public string GetString(string op, Dictionary<string, string> @params, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
    {
      var builder = new UriBuilder(this.uri + op);
      NameValueCollection queryString = HttpUtility.ParseQueryString(String.Empty);
      foreach (KeyValuePair<string, string> x in @params)
      {
        queryString[x.Key] = x.Value;
      }
      builder.Query = queryString.ToString();
      var c = OpenHttpConnection(builder.Uri, "GET");
      c = auth.Prepare(c);
      c.ContentType = mimeRequest == null ? "text/plain; charset=utf-8" : mimeRequest + "; charset=utf-8";
      c.Accept = mimeResponse == null ? "text/plain; charset=utf-8" : mimeResponse + "; charset=utf-8";

      using (var resp = (HttpWebResponse)c.GetResponse())
      {
        var sr = new StreamReader(resp.GetResponseStream());
        return sr.ReadToEnd();
      }
   
    }

    /// <summary>
    /// Make a call with the given operation and post to the uri. Response is returned as a string.
    /// </summary>
    /// <param name="op">Given operation</param>
    /// <param name="req">Properly formatted request string</param>
    /// <param name="mimeRequest">Mime type for ContentType header</param>
    /// <param name="mimeResponse">Mime type for Accept header</param>
    /// <returns>Raw string of the result</returns>
    public string PostString(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
    {
      var builder = new UriBuilder(this.uri + op);
      var c = OpenHttpConnection(builder.Uri, "POST");
      c = auth.Prepare(c);
      c.Method = "POST";
      c.ContentType = mimeRequest == null ? "text/plain; charset=utf-8" : mimeRequest + "; charset=utf-8";
      c.Accept = mimeResponse == null ? "text/plain; charset=utf-8" : mimeResponse + "; charset=utf-8";
      byte[] data = Encoding.ASCII.GetBytes(req);
      c.ContentLength = data.Length;
      Stream stream = c.GetRequestStream();
      stream.Write(data, 0, data.Length);
      stream.Close();
      WebResponse webResp = c.GetResponse();
      stream = webResp.GetResponseStream();
      StreamReader sr = new StreamReader(stream);

      return sr.ReadToEnd();
    }


    ////////////////////////////////////////////////////////////////
    // Utils
    ////////////////////////////////////////////////////////////////

    private HttpWebRequest OpenHttpConnection(Uri url, string method)
    {
      try
      {
        return OpenHttpConnection(url, method, this.connectTimeout, this.readTimeout);
      }
      catch (IOException e)
      {
        throw e;
      }
    }

    public static HttpWebRequest OpenHttpConnection(Uri url, string method, int connectTimeout, int readTimeout)
    {
      try
      {
        HttpWebRequest c = (HttpWebRequest)WebRequest.Create(url);
        c.Method = method;
        c.AllowAutoRedirect = false;
        c.Timeout = connectTimeout;
        c.ReadWriteTimeout = readTimeout;
        return c;
      }
      catch (IOException e)
      {
        throw e;
      }
    }

    ////////////////////////////////////////////////////////////////
    // Property
    ////////////////////////////////////////////////////////////////

    internal class Property
    {
      internal Property(string key, string value)
      {
        this.key = key;
        this.value = value;
      }

      public override string ToString()
      {
        return "[Property " + "key:" + key + ", " + "value:" + value + "]";
      }

      internal readonly string key;
      internal readonly string value;
    }

    ////////////////////////////////////////////////////////////////
    // main
    ////////////////////////////////////////////////////////////////
    
    internal static HClient MakeClient(string uri, string user, string pass)
    {
      // create proper client
      try
      {
        return HClient.Open(uri, user, pass);
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public static void Main(string[] args)
    {
      try
      {
        if (args.Length != 3)
        {
          Console.WriteLine("usage: HClient <uri> <user> <pass>");
          Environment.Exit(0);
        }

        HClient client = MakeClient(args[0], args[1], args[2]);
        Console.WriteLine(client.GetString("about", new Dictionary<string, string>(), "text/zinc"));
        Console.ReadKey();
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    //////////////////////////////////////////////////////////////////////////
    // Fields
    //////////////////////////////////////////////////////////////////////////

    private AuthClientContext auth;
    private Hashtable watches_Renamed = new Hashtable();

  }
}