using System;
using System.Collections;
using System.Net;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
  public interface IAuthClientContext
  {
    string uri { get; }
    string user { get; }
    string pass { get; }
    IDictionary headers { get; }
    IDictionary stash { get; }

    //HttpWebRequest OpenHttpConnection(string uri, string method);
    HttpWebRequest Prepare(HttpWebRequest c);
    void AddCookiesToHeaders(HttpWebRequest c);

    ServerCallAsync ServerCallAsync { get; }
    ServerCall ServerCall { get; }
  }

  public delegate Task<HttpWebResponse> ServerCallAsync(string action, Action<HttpWebRequest> requestConfigurator);

  public delegate HttpWebResponse ServerCall(string action, Action<HttpWebRequest> requestConfigurator);
}