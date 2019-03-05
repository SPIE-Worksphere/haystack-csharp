using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using ProjectHaystack.Auth;

namespace ProjectHaystackTest.Helpers
{
  public class MockServerCallBuilder
  {
    bool _failingLogin = false;
    string _readResponse = null;

    public MockServerCallBuilder WithFailingLogin()
    {
      _failingLogin = true;
      return this;
    }

    public MockServerCallBuilder WithReadResponse(string response)
    {
      _readResponse = response;
      return this;
    }

    public ServerCallAsync Build()
    {
      bool isLoggedIn = false;
      ServerCallAsync handler = (string action, Action<HttpWebRequest> requestConfigurator) =>
      {
        var request = (HttpWebRequest)WebRequest.Create("http://localhost/" + action);
        requestConfigurator(request);

        var message = new HttpResponseMessage(HttpStatusCode.OK);
        switch (action)
        {
          case "about":
            if (_failingLogin)
              message.StatusCode = HttpStatusCode.Unauthorized;
            else
              isLoggedIn = true;
            break;
          case "read":
            if (!isLoggedIn)
              throw new WebException("Not logged in");
            message.Content = new StringContent(_readResponse);
            break;
        }

        var ctor = typeof(HttpWebResponse).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(HttpResponseMessage), typeof(Uri), typeof(CookieContainer) }, null);
        var response = (HttpWebResponse)ctor.Invoke(new object[] { message, new Uri("http://localhost"), new CookieContainer() });
        return Task.FromResult(response);
      };
      return handler;
    }
  }
}