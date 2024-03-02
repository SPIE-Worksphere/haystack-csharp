using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProjectHaystack.Auth;
using ProjectHaystack.Client;

namespace ProjectHaystack.Examples.Client
{
    /// <summary>
    /// Examples on how to set up an authenticated connection.
    /// If your system does not support any of the known authenticators,
    /// you can write your own by implementing IAuthenticator.
    /// Please be so kind to share your implementation with the world by adding it to this library.
    /// </summary>
    public class AuthenticationExamples
    {
        /// <summary>
        /// Connect using scram authentication.
        /// This will set a cookie on the HttpClient.
        /// </summary>
        public async Task ConnectUsingScramAuthentication()
        {
            var user = "someuser";
            var pass = "somepassword";
            var uri = new Uri("https://someserver/api/");
            var auth = new ScramAuthenticator(user, pass);
            // When authentications fails in certain legacy systems, set this value to true.
            auth.AddLegacySpaceToProof = true;
            var client = new HaystackClient(auth, uri);
            await client.OpenAsync();
        }

        /// <summary>
        /// Connect using basic authentication.
        /// This will usually set a cookie on the HttpClient.
        /// </summary>
        public async Task ConnectUsingBasicAuthentication()
        {
            var user = "someuser";
            var pass = "somepassword";
            var uri = new Uri("https://someserver/api/");
            var auth = new BasicAuthenticator(user, pass);
            var client = new HaystackClient(auth, uri);
            await client.OpenAsync();
        }

        /// <summary>
        /// Auto-detect authentication scheme.
        /// </summary>
        public async Task ConnectUsingAutodetectAuthentication()
        {
            var user = "someuser";
            var pass = "somepassword";
            var uri = new Uri("https://someserver/api/");
            var auth = new AutodetectAuthenticator(user, pass);
            var client = new HaystackClient(auth, uri);
            await client.OpenAsync();
        }

        /// <summary>
        /// Provide your own HttpClient to get more control over the connection.
        /// </summary>
        public async Task ConnectUsingSharedHttpClient()
        {
            var user = "someuser";
            var pass = "somepassword";
            var uri = new Uri("https://someserver/api/");
            var auth = new AutodetectAuthenticator(user, pass);
            var httpClientHandler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false };
            var httpClient = new HttpClient(httpClientHandler);
            var client = new HaystackClient(httpClient, auth, uri);
            await client.OpenAsync();
        }
    }
}