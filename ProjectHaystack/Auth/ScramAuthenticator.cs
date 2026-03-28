using ProjectHaystack.Auth.Util;
using ProjectHaystack.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
    /// <summary>
    /// Authentication mechanism using SCRAM (Salted Challenge Response Authentication Mechanism).
    /// </summary>
    public class ScramAuthenticator : IAuthenticator
    {
        private const string _gs2Header = "n,,";
        private const int _clientNonceBytes = 16;
        private const string _wwwAuthenticateHeader = "WWW-Authenticate";

        private readonly string _username;
        private readonly string _password;

        private string _cnonce;
        private string _bare;
        private IDictionary<string, string> _lastMessage;

        public ScramAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        /// <summary>
        /// According to RFC7804 and RFC5802 the client proof should not be sent with a space at the start of the parameter,
        /// however, some systems use some legacy behavior where this space is required for the authentication to succeed.
        /// When authentications fails in these legacy systems, set this value to true to rectify the problem.
        /// </summary>
        public bool AddLegacySpaceToProof { get; set; }

        public async Task Authenticate(HttpClient client, Uri authUrl)
        {
            _cnonce = GenNonce();
            _bare = $"n={_username},r={_cnonce}";

            await SendHello(client, authUrl).ConfigureAwait(false);
            await SendFirst(client, authUrl).ConfigureAwait(false);
            await SendFinal(client, authUrl).ConfigureAwait(false);
        }

        private async Task SendHello(HttpClient client, Uri authUrl)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("HELLO",
                "username=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(_username)).Trim('='));
            using (var response = await client.SendAsync(message))
            {
                string auth = null;
                try
                {
                    auth = response.Headers.GetValues(_wwwAuthenticateHeader).First();
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException($"Cannot get authentication header, server response was: {(int)response.StatusCode}");
                }

                _lastMessage = ScramSha256Helper.TokenToDict(auth.Substring(6));
            }
        }

        private async Task SendFirst(HttpClient client, Uri authUrl)
        {
            _bare = "n=" + _username + ",r=" + _cnonce;

            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("scram",
               ScramSha256Helper.DictToToken(new Dictionary<string, string>
               {
                   ["data"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(_gs2Header + _bare)).Trim('='),
                   ["handshakeToken"] = _lastMessage["handshakeToken"],
               }));
            using (var response = await client.SendAsync(message))
            {
                string auth = null;
                try
                {
                    auth = response.Headers.GetValues(_wwwAuthenticateHeader).First();
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException($"Cannot get authentication header, server response was: {(int)response.StatusCode}");
                }
                _lastMessage = ScramSha256Helper.TokenToDict(auth.Substring(6));
            }
        }

        private async Task SendFinal(HttpClient client, Uri authUrl)
        {
            // Decode server-first-message
            var s1_msg = Encoding.UTF8.GetString(ScramSha256Helper.FromBase64String(_lastMessage["data"]));
            var data = ScramSha256Helper.TokenToDict(s1_msg);

            // c2-no-proof
            var c2_no_proof = ScramSha256Helper.DictToToken(new Dictionary<string, string>
            {
                ["c"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(_gs2Header)),
                ["r"] = data["r"],
            });

            // proof
            var hash = _lastMessage["hash"];
            var salt = data["s"];
            var iterations = int.Parse(data["i"]);
            var authMsg = _bare + "," + s1_msg + "," + c2_no_proof;

            var saltedPassword = ScramSha256Helper.ComputeSaltedPassword(hash, _password, salt, iterations);
            var clientProof = ScramSha256Helper.CreateClientProof(saltedPassword, Encoding.UTF8.GetBytes(authMsg), AddLegacySpaceToProof);

            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("scram",
               ScramSha256Helper.DictToToken(new Dictionary<string, string>
               {
                   ["data"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(c2_no_proof + ",p=" + clientProof)).Trim('='),
                   ["handshakeToken"] = _lastMessage["handshakeToken"],
               }));
            var response = await client.SendAsync(message);
            string auth = null;
            try
            {
                auth = response.Headers.GetValues("Authentication-Info").First();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"Cannot get authentication header, server response was: {(int)response.StatusCode}");
            }
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "authToken=" + ScramSha256Helper.TokenToDict(auth)["authToken"]);
            response.Dispose();
        }



        /// <summary>
        /// Generate a random nonce string </summary>
        private string GenNonce()
        {
            byte[] bytes = new byte[_clientNonceBytes * 2];
            RandomNumberGenerator rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            return new string(Convert.ToBase64String(bytes)
                .Where(chr => allowed.Contains(chr))
                .Take(_clientNonceBytes)
                .ToArray());
        }
    }
}