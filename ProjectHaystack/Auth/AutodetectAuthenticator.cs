using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
    public class AutodetectAuthenticator : IAuthenticator
    {
        private readonly string _username;
        private readonly string _password;
        private const string _wwwAuthenticateHeader = "WWW-Authenticate";

        public AutodetectAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public async Task Authenticate(HttpClient client, Uri authUrl)
        {
            var authenticator = await SendHello(client, authUrl).ConfigureAwait(false);
            await authenticator.Authenticate(client, authUrl);
        }

        private async Task<IAuthenticator> SendHello(HttpClient client, Uri authUrl)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("HELLO",
                "username=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username)).Trim('='));
            using (var response = await client.SendAsync(message))
            {
                var auth = response.Headers.GetValues(_wwwAuthenticateHeader).First();
                var authLower = auth.ToLower();

                if (authLower.StartsWith("basic"))
                    return new BasicAuthenticator(_username, _password);

                if (authLower.StartsWith("scram"))
                    return new ScramAuthenticator(_username, _password);

                throw new InvalidOperationException($"Autodetect cannot determine authentication type from authentication header: {auth}");
            }
        }
    }
}