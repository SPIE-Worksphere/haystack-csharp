using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
    /// <summary>
    /// Authentication method using HayStack Basic authentication.
    /// </summary>
    public class BasicAuthenticator : IAuthenticator
    {
        private readonly string _username;
        private readonly string _password;
        private string _lastMessage;
        private const string _wwwAuthenticateHeader = "WWW-Authenticate";

        public BasicAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public async Task Authenticate(HttpClient client, Uri authUrl)
        {
            await SendHello(client, authUrl).ConfigureAwait(false);
            await SendFinal(client, authUrl).ConfigureAwait(false);
        }

        private async Task SendHello(HttpClient client, Uri authUrl)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("HELLO",
                "username=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username)).Trim('='));
            using (var response = await client.SendAsync(message))
            {
                var auth = response.Headers.GetValues(_wwwAuthenticateHeader).First();
                _lastMessage = auth.ToLower();
                if (_lastMessage.StartsWith("basic"))
                    return;

                string server = response.Headers.GetValues("Server").FirstOrDefault()?.ToLower() ?? string.Empty;
                // fallback to basic if server says it's Niagara; Niagara 4.6 return empry WWW-Authenticate and Server headers
                if (server.StartsWith("niagara", StringComparison.Ordinal) || (response.StatusCode == HttpStatusCode.Unauthorized && string.IsNullOrEmpty(auth) && string.IsNullOrEmpty(server)))
                    return;

                // detect N4 by their bug - lolol
                var content = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.InternalServerError && content != null && content.Contains("wrong 4-byte ending"))
                    return;

                throw new InvalidOperationException("Invalid reponse on basic authentication request");
            }
        }

        private async Task SendFinal(HttpClient client, Uri authUrl)
        {
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
                message.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _password)).Trim('='));
                using (var response = await client.SendAsync(message))
                {
                    if ((int)response.StatusCode != 200)
                    {
                        throw new AuthException("Basic auth failed: " + response.StatusCode + " " + (await response.Content.ReadAsStringAsync()));
                    }
                }
            }
            catch (Exception e)
            {
                throw new AuthException("basic authentication failed", e);
            }
        }
    }
}