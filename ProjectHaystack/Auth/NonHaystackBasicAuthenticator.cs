using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
    /// <summary>
    /// Authentication mechanism using non-Haystack simple Basic authentication.
    /// </summary>
    public class NonHaystackBasicAuthenticator : IAuthenticator
    {
        private readonly string _username;
        private readonly string _password;

        public NonHaystackBasicAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public async Task Authenticate(HttpClient client, Uri authUrl)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(_username + ":" + _password)).Trim('='));
            try
            {
                using (var response = await client.GetAsync(authUrl))
                {
                    if ((int)response.StatusCode != 200)
                    {
                        throw new HaystackAuthException("Basic auth failed: " + response.StatusCode + " " + (await response.Content.ReadAsStringAsync()));
                    }
                }
            }
            catch (Exception e)
            {
                throw new HaystackAuthException("basic authentication failed", e);
            }
        }
    }
}