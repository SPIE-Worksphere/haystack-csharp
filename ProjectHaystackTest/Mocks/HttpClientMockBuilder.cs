using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using ProjectHaystack;
using ProjectHaystack.Auth.Util;
using ProjectHaystack.io;

namespace ProjectHaystackTest.Mocks
{
    public class HttpClientMockBuilder
    {
        private readonly Uri _baseUri;
        private readonly Mock<HttpMessageHandler> _httpMessageHanderMock = new Mock<HttpMessageHandler>();
        private readonly List<Func<HttpRequestMessage, Task<HttpResponseMessage>>> _requestHandlers = new List<Func<HttpRequestMessage, Task<HttpResponseMessage>>>();

        public HttpClientMockBuilder(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public HttpClient Build()
        {
            _httpMessageHanderMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns<HttpRequestMessage, CancellationToken>((request, _) =>
                {
                    foreach (var requestHandler in _requestHandlers)
                    {
                        var response = requestHandler(request);
                        if (response != null)
                        {
                            return response;
                        }
                    }
                    throw new Exception("Unexpected request");
                });
            return new HttpClient(_httpMessageHanderMock.Object);
        }

        public HttpClientMockBuilder WithBasicAuthentication(string allowedBasicAuth)
        {
            _requestHandlers.Add(request =>
            {
                if (request.Headers.Authorization == null)
                {
                    return Task.FromResult<HttpResponseMessage>(null);
                }
                if (request.Headers.Authorization.Scheme == "HELLO")
                {
                    var response = new HttpResponseMessage();
                    response.Headers.Add("WWW-Authenticate", "basic");
                    return Task.FromResult(response);
                }
                if (request.Headers.Authorization.Scheme == "Basic" && request.RequestUri == new Uri(_baseUri, "about"))
                {
                    if (request.Headers.Authorization.Parameter == allowedBasicAuth)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                    }
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
                }
                return Task.FromResult<HttpResponseMessage>(null);
            });

            return this;
        }

        public HttpClientMockBuilder WithScramAuthentication(string allowedUsername, string allowedPassword)
        {
            string storedUsername = null;
            string clientNonce = null;
            string serverNonce = "mockServerNonce";
            string salt = "mockSalt";
            int iterations = 4096;
            string clientFirstMessageBare = null;

            _requestHandlers.Add(request =>
            {
                if (request.Headers.Authorization == null)
                {
                    return null;
                }

                var authHeader = request.Headers.Authorization;

                if (authHeader.Scheme == "HELLO")
                {
                    return HandleScramHelloRequest(authHeader, allowedUsername, ref storedUsername);
                }

                if (authHeader.Scheme.Equals("scram", StringComparison.OrdinalIgnoreCase))
                {
                    var parameters = ScramSha256Helper.TokenToDict(authHeader.Parameter);
                    if (!parameters.TryGetValue("data", out var dataBytes))
                    {
                        return null;
                    }

                    var decodedData = Encoding.UTF8.GetString(ScramSha256Helper.FromBase64String(dataBytes));

                    if (!decodedData.Contains(",p="))
                    {
                        return HandleScramClientFirstMessage(
                            decodedData,
                            allowedUsername,
                            serverNonce,
                            salt,
                            iterations,
                            ref clientNonce,
                            ref clientFirstMessageBare);
                    }

                    if (request.RequestUri == new Uri(_baseUri, "about"))
                    {
                        return HandleScramClientFinalMessage(
                            decodedData,
                            storedUsername,
                            allowedUsername,
                            clientNonce,
                            clientFirstMessageBare,
                            serverNonce,
                            salt,
                            iterations,
                            allowedPassword);
                    }
                }

                return null;
            });

            return this;
        }

        public HttpClientMockBuilder WithReadAsync(string expectedFilter, HaystackGrid response)
        {
            _requestHandlers.Add(async request =>
            {
                var relativeUri = _baseUri.MakeRelativeUri(request.RequestUri);
                if (relativeUri.OriginalString != "read")
                {
                    return null;
                }
                var reader = new ZincReader(await request.Content.ReadAsStringAsync());
                var grid = reader.ReadValue<HaystackGrid>();
                var filter = grid.Rows.First().Get<HaystackString>("filter").Value;
                if (filter != expectedFilter)
                {
                    return null;
                }
                using (var stream = new MemoryStream())
                using (var streamWriter = new StreamWriter(stream))
                {
                    var writer = new ZincWriter(streamWriter);
                    writer.WriteValue(response);
                    streamWriter.Flush();
                    stream.Position = 0;
                    return new HttpResponseMessage
                    {
                        Content = new StringContent(await new StreamReader(stream).ReadToEndAsync()),
                    };
                }
            });

            return this;
        }

        public HttpClientMockBuilder WithRequestHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handleRequest)
        {
            _requestHandlers.Add(handleRequest);

            return this;
        }

        #region Private Methods

        private Task<HttpResponseMessage> HandleScramHelloRequest(
            AuthenticationHeaderValue authHeader,
            string allowedUsername,
            ref string storedUsername)
        {
            var response = new HttpResponseMessage();
            var parameterPrefix = "username=";

            if (authHeader.Parameter.StartsWith(parameterPrefix))
            {
                var base64Username = authHeader.Parameter.Substring(parameterPrefix.Length);
                var username = Encoding.UTF8.GetString(ScramSha256Helper.FromBase64String(base64Username));

                if (username != allowedUsername)
                {
                    response.StatusCode = HttpStatusCode.Unauthorized;
                    response.Headers.Add("WWW-Authenticate", "SCRAM hash=SHA-256, handshakeToken=invalid");
                    return Task.FromResult(response);
                }

                storedUsername = username;
            }

            response.Headers.Add("WWW-Authenticate", "SCRAM hash=SHA-256, handshakeToken=mockHandshakeToken");
            return Task.FromResult(response);
        }

        private Task<HttpResponseMessage> HandleScramClientFirstMessage(
            string decodedData,
            string allowedUsername,
            string serverNonce,
            string salt,
            int iterations,
            ref string clientNonce,
            ref string clientFirstMessageBare)
        {
            var parts = decodedData.Split(new[] { "n,," }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return null;
            }

            clientFirstMessageBare = parts[1];
            var usernameMatch = Regex.Match(clientFirstMessageBare, @"n=([^,]+)");
            var nonceMatch = Regex.Match(clientFirstMessageBare, @"r=([^,]+)");

            if (usernameMatch.Success && nonceMatch.Success)
            {
                var scramUsername = usernameMatch.Groups[1].Value;
                clientNonce = nonceMatch.Groups[1].Value;

                if (scramUsername != allowedUsername)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return Task.FromResult(response);
                }
            }

            var serverFirstMessage = $"r={clientNonce}{serverNonce},s={salt},i={iterations}";
            var serverFirstMessageBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(serverFirstMessage)).TrimEnd('=');
            var resp = new HttpResponseMessage();
            resp.Headers.Add("WWW-Authenticate", $"SCRAM hash=SHA-256, data={serverFirstMessageBase64}, handshakeToken=mockHandshakeToken");
            return Task.FromResult(resp);
        }

        private Task<HttpResponseMessage> HandleScramClientFinalMessage(
            string decodedData,
            string storedUsername,
            string allowedUsername,
            string clientNonce,
            string clientFirstMessageBare,
            string serverNonce,
            string salt,
            int iterations,
            string allowedPassword)
        {
            if (storedUsername != allowedUsername || string.IsNullOrEmpty(clientNonce) || string.IsNullOrEmpty(clientFirstMessageBare))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            var clientFinalMatch = Regex.Match(decodedData, @"p=([^,]+)");
            var channelBindingMatch = Regex.Match(decodedData, @"c=([^,]+)");
            var nonceMatch = Regex.Match(decodedData, @"r=([^,]+)");

            if (!clientFinalMatch.Success || !channelBindingMatch.Success || !nonceMatch.Success)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            var receivedProof = clientFinalMatch.Groups[1].Value.TrimEnd('=');
            var channelBinding = channelBindingMatch.Groups[1].Value;
            var fullNonce = nonceMatch.Groups[1].Value;
            var clientFinalWithoutProof = $"c={channelBinding},r={fullNonce}";
            var serverFirstMessage = $"r={fullNonce},s={salt},i={iterations}";
            var authMessage = $"{clientFirstMessageBare},{serverFirstMessage},{clientFinalWithoutProof}";

            var saltedPassword = ScramSha256Helper.ComputeSaltedPassword("SHA-256", allowedPassword, salt, iterations);
            var expectedProof = ScramSha256Helper.CreateClientProof(saltedPassword, Encoding.UTF8.GetBytes(authMessage)).TrimEnd('=');

            if (receivedProof != expectedProof)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            var authResponseHeader = "authToken=mockAuthToken";
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("Authentication-Info", authResponseHeader);
            return Task.FromResult(response);
        }

        #endregion
    }
}