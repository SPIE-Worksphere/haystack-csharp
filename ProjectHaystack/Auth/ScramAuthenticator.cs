using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ProjectHaystack.Util;

namespace ProjectHaystack.Auth
{
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
                var auth = response.Headers.GetValues(_wwwAuthenticateHeader).First();
                _lastMessage = TokenToDict(auth.Substring(6));
            }
        }

        private async Task SendFirst(HttpClient client, Uri authUrl)
        {
            _bare = "n=" + _username + ",r=" + _cnonce;

            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("scram",
               DictToToken(new Dictionary<string, string>
               {
                   ["data"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(_gs2Header + _bare)).Trim('='),
                   ["handshakeToken"] = _lastMessage["handshakeToken"],
               }));
            using (var response = await client.SendAsync(message))
            {
                var auth = response.Headers.GetValues(_wwwAuthenticateHeader).First();
                _lastMessage = TokenToDict(auth.Substring(6));
            }
        }

        private async Task SendFinal(HttpClient client, Uri authUrl)
        {
            // Decode server-first-message
            var s1_msg = Encoding.UTF8.GetString(Convert.FromBase64String(_lastMessage["data"]));
            var data = TokenToDict(s1_msg);

            // c2-no-proof
            var c2_no_proof = DictToToken(new Dictionary<string, string>
            {
                ["c"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(_gs2Header)),
                ["r"] = data["r"],
            });

            // proof
            var hash = _lastMessage["hash"];
            var salt = data["s"];
            var iterations = int.Parse(data["i"]);
            var authMsg = _bare + "," + s1_msg + "," + c2_no_proof;

            var saltedPassword = Pbk(hash, _password, salt, iterations);
            var clientProof = CreateClientProof(saltedPassword, Encoding.UTF8.GetBytes(authMsg));

            var message = new HttpRequestMessage(HttpMethod.Get, authUrl);
            message.Headers.Authorization = new AuthenticationHeaderValue("scram",
               DictToToken(new Dictionary<string, string>
               {
                   ["data"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(c2_no_proof + ",p= " + clientProof)),
                   ["handshakeToken"] = _lastMessage["handshakeToken"],
               }));
            var response = await client.SendAsync(message);
            var auth = response.Headers.GetValues("Authentication-Info").First();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", "authToken=" + TokenToDict(auth)["authToken"]);
            response.Dispose();
        }

        private IDictionary<string, string> TokenToDict(string token) =>
            token.Split(',')
                .Select(s => s.Split(new[] { '=' }, 2).Select(v => v.Trim()).ToArray())
                .ToDictionary(a => a[0], a => a[1]);

        private string DictToToken(IDictionary<string, string> dict) =>
             string.Join(",", dict.Where(kv => kv.Value != null).Select(kv => $"{kv.Key}={kv.Value}"));

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

        private static sbyte[] Pbk(string hash, string password, string salt, int iterations)
        {
            try
            {
                byte[] saltBytes = Convert.FromBase64String(salt);
                using (var hmac = new HMACSHA256())
                {
                    var mine = new Pbkdf2(hmac, Encoding.UTF8.GetBytes(password),
                      (byte[])(Array)Convert.FromBase64String(salt), iterations);
                    sbyte[] signed = mine.GetBytes(32);
                    byte[] signednew = (byte[])(Array)signed;
                    return signed;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static string CreateClientProof(sbyte[] saltedPassword, byte[] authMsg)
        {
            try
            {
                using (var hmac = new HMACSHA256())
                {
                    byte[] usSaltedPassword = (byte[])(Array)saltedPassword;
                    byte[] usAuthMsg = authMsg;
                    var hmac2 = new HMACSHA256(usSaltedPassword);
                    byte[] clientKey = hmac2.ComputeHash(Encoding.UTF8.GetBytes("Client Key"));
                    var sha1 = new SHA256Managed();
                    byte[] storedKey = sha1.ComputeHash(clientKey);
                    var hmac3 = new HMACSHA256((byte[])(Array)storedKey);
                    byte[] clientSig = hmac3.ComputeHash(authMsg);
                    byte[] clientProof = new byte[clientKey.Length];
                    for (int i = 0; i < clientKey.Length; i++)
                    {
                        clientProof[i] = (byte)(clientKey[i] ^ clientSig[i]);
                    }
                    return Convert.ToBase64String(clientProof.Cast<byte>().ToArray());
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}