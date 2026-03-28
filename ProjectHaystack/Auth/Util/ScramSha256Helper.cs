using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ProjectHaystack.Util;

namespace ProjectHaystack.Auth.Util
{
    /// <summary>
    /// Helper class for SCRAM-SHA-256 authentication cryptographic operations
    /// </summary>
    public static class ScramSha256Helper
    {
        /// <summary>
        /// Some haystack servers can trim = from base64, so we need to restore it.
        /// Otherwise FromBase64String will throw exception.
        /// </summary>
        public static byte[] FromBase64String(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var sb = new StringBuilder(s);
                while ((sb.Length * 6) % 8 != 0) sb.Append("=");
                s = sb.ToString();
            }
            return Convert.FromBase64String(s);
        }

        /// <summary>
        /// Computes the salted password using PBKDF2
        /// </summary>
        /// <param name="hash">Hash algorithm name (e.g., "SHA-256")</param>
        /// <param name="password">User password</param>
        /// <param name="salt">Base64-encoded salt</param>
        /// <param name="iterations">Number of iterations</param>
        /// <returns>Salted password as signed byte array</returns>
        public static sbyte[] ComputeSaltedPassword(string hash, string password, string salt, int iterations)
        {
            byte[] saltBytes = FromBase64String(salt);
            using (var hmac = new HMACSHA256())
            {
                var pbkdf2 = new Pbkdf2(hmac, Encoding.UTF8.GetBytes(password), saltBytes, iterations);
                return pbkdf2.GetBytes(32);
            }
        }

        /// <summary>
        /// Creates the client proof for SCRAM authentication
        /// </summary>
        /// <param name="saltedPassword">The salted password from PBKDF2</param>
        /// <param name="authMsg">The authentication message</param>
        /// <returns>Base64-encoded client proof</returns>
        public static string CreateClientProof(sbyte[] saltedPassword, byte[] authMsg)
        {
            using (var hmac = new HMACSHA256())
            {
                byte[] usSaltedPassword = (byte[])(Array)saltedPassword;

                // Compute Client Key = HMAC(saltedPassword, "Client Key")
                var hmac2 = new HMACSHA256(usSaltedPassword);
                byte[] clientKey = hmac2.ComputeHash(Encoding.UTF8.GetBytes("Client Key"));

                // Compute Stored Key = SHA256(Client Key)
                var sha256 = new SHA256Managed();
                byte[] storedKey = sha256.ComputeHash(clientKey);

                // Compute Client Signature = HMAC(Stored Key, auth message)
                var hmac3 = new HMACSHA256(storedKey);
                byte[] clientSig = hmac3.ComputeHash(authMsg);

                // Compute Client Proof = Client Key XOR Client Signature
                byte[] clientProof = new byte[clientKey.Length];
                for (int i = 0; i < clientKey.Length; i++)
                {
                    clientProof[i] = (byte)(clientKey[i] ^ clientSig[i]);
                }

                return Convert.ToBase64String(clientProof.Cast<byte>().ToArray());
            }
        }

        /// <summary>
        /// Creates the client proof with optional legacy space prefix
        /// </summary>
        /// <param name="saltedPassword">The salted password from PBKDF2</param>
        /// <param name="authMsg">The authentication message</param>
        /// <param name="addLegacySpace">Whether to add a space prefix for legacy systems</param>
        /// <returns>Base64-encoded client proof</returns>
        public static string CreateClientProof(sbyte[] saltedPassword, byte[] authMsg, bool addLegacySpace)
        {
            var proof = CreateClientProof(saltedPassword, authMsg);
            return addLegacySpace ? " " + proof : proof;
        }

        /// <summary>
        /// Parses a token string into a dictionary
        /// </summary>
        /// <param name="token">Token string with format: key1=value1,key2=value2</param>
        /// <returns>Dictionary of key-value pairs</returns>
        public static System.Collections.Generic.IDictionary<string, string> TokenToDict(string token)
        {
            return token.Split(',')
                .Select(s => s.Split(new[] { '=' }, 2).Select(v => v.Trim()).ToArray())
                .ToDictionary(a => a[0], a => a.Count() > 1 ? a[1] : string.Empty);
        }

        /// <summary>
        /// Converts a dictionary to a token string
        /// </summary>
        /// <param name="dict">Dictionary of key-value pairs</param>
        /// <returns>Token string with format: key1=value1,key2=value2</returns>
        public static string DictToToken(System.Collections.Generic.IDictionary<string, string> dict)
        {
            return string.Join(",", dict.Where(kv => kv.Value != null).Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
