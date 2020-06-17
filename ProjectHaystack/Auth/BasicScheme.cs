//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
  using Base64 = ProjectHaystack.Util.Base64;

  /// <summary>
  /// BasicScheme
  /// </summary>
  public sealed class BasicScheme : AuthScheme
  {
    public BasicScheme()
      : base("basic") { }

    public override AuthMsg OnClient(IAuthClientContext cx, AuthMsg msg)
    {
      throw new System.NotSupportedException();
    }

    public override bool OnClientNonStd(IAuthClientContext cx, HttpWebResponse resp, string content)
    {
      if (!Use(resp, content))
      {
        return false;
      }

      string cred = Base64.STANDARD.EncodeUtf8(cx.user + ":" + cx.pass);

      // make another qrequest to verify
      string headerKey = "Authorization";
      string headerVal = "Basic " + cred;

      try
      {
        // make another request to verify
        HttpWebRequest origRequest = null;
        var resp2 = cx.ServerCall("about", c =>
        {
          c.Headers.Add(HttpRequestHeader.Authorization, headerVal);
          origRequest = c;
        });
        resp2.GetResponseHeader(headerKey);
        if ((int)resp2.StatusCode != 200)
        {
          throw new AuthException("Basic auth failed: " + resp2.StatusCode + " " + resp2.GetResponseStream().ToString());
        }

        // pass Authorization and Cookie headers for future requests
        cx.headers[headerKey] = headerVal;
        cx.AddCookiesToHeaders(origRequest);
        return true;
      }
      catch (Exception e)
      {
        throw new AuthException("basic authentication failed", e);
      }
    }

    public override async Task<bool> OnClientNonStdAsync(IAuthClientContext cx, HttpWebResponse resp, string content)
    {
      if (!Use(resp, content))
      {
        return false;
      }

      string cred = Base64.STANDARD.EncodeUtf8(cx.user + ":" + cx.pass);

      // make another qrequest to verify
      string headerKey = "Authorization";
      string headerVal = "Basic " + cred;

      try
      {
        // make another request to verify
        HttpWebRequest origRequest = null;
        using (var resp2 = await cx.ServerCallAsync("about", c =>
        {
          c.Headers.Add(HttpRequestHeader.Authorization, headerVal);
          origRequest = c;
        }))
        {
          resp2.GetResponseHeader(headerKey);
          if ((int)resp2.StatusCode != 200)
          {
            throw new AuthException("Basic auth failed: " + resp2.StatusCode + " " + resp2.GetResponseStream().ToString());
          }

          // pass Authorization and Cookie headers for future requests
          cx.headers[headerKey] = headerVal;
          cx.AddCookiesToHeaders(origRequest);
          return true;
        }
      }
      catch (Exception e)
      {
        throw new AuthException("basic authentication failed", e);
      }
    }

    public static bool Use(HttpWebResponse c, string content)
    {
      try
      {
        int resCode = (int)c.StatusCode;

        string wwwAuth = c.GetResponseHeader("WWW-Authenticate");
        if (string.ReferenceEquals(wwwAuth, null))
        {
          wwwAuth = "";
        }
        wwwAuth = wwwAuth.ToLower();

        string server = c.GetResponseHeader("Server");
        if (string.ReferenceEquals(server, null))
        {
          server = "";
        }
        server = server.ToLower();

        // standard basic challenge
        if (resCode == 401 && wwwAuth.StartsWith("basic", StringComparison.Ordinal))
        {
          return true;
        }

        // fallback to basic if server says it's Niagara; Niagara 4.6 return empry WWW-Authenticate and Server headers
        if (server.StartsWith("niagara", StringComparison.Ordinal) || (resCode == 401 && string.IsNullOrEmpty(wwwAuth) && string.IsNullOrEmpty(server)))
        {
          return true;
        }

        // detect N4 by their bug - lolol
        if (resCode == 500 && !string.ReferenceEquals(content, null) && content.Contains("wrong 4-byte ending"))
        {
          return true;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        Console.Write(e.StackTrace);
      }
      return false;
    }
  }

}