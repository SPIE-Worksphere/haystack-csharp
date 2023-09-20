using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using ProjectHaystack;
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
                    return null;
                }
                if (request.Headers.Authorization.Scheme == "HELLO")
                {
                    var response = new HttpResponseMessage();
                    response.Headers.Add("WWW-Authenticate", "basic");
                    return Task.FromResult(response);
                }
                if (request.Headers.Authorization.Scheme == "Basic" && request.RequestUri.LocalPath == "/user/auth")
                {
                    if (request.Headers.Authorization.Parameter == allowedBasicAuth)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                    }
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
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
    }
}