using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.Auth;
using ProjectHaystack.Client;
using ProjectHaystackTest.Mocks;

namespace ProjectHaystackTest.Client.Tests
{
    [TestClass]
    public class HaystackClientTest
    {
        private static Uri _uri = new Uri("http://localhost:8080/api/demo/");
        private const string _user = "User";
        private const string _pass = "Pass";
        private const string _basicAuthHash = "VXNlcjpQYXNz";

        [TestMethod]
        public async Task LoginTest()
        {
            var httpClient = new HttpClientMockBuilder(_uri)
                .WithBasicAuthentication(_basicAuthHash)
                .Build();
            var client = new HaystackClient(httpClient, new BasicAuthenticator(_user, _pass), _uri);
            await client.OpenAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HaystackAuthException))]
        public async Task BadUserTest()
        {
            var httpClient = new HttpClientMockBuilder(_uri)
                .WithBasicAuthentication(_basicAuthHash)
                .Build();
            var client = new HaystackClient(httpClient, new BasicAuthenticator("wrongUser", _pass), _uri);
            await client.OpenAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HaystackAuthException))]
        public async Task BadPassTest()
        {
            var httpClient = new HttpClientMockBuilder(_uri)
                .WithBasicAuthentication(_basicAuthHash)
                .Build();
            var client = new HaystackClient(httpClient, new BasicAuthenticator(_user, "wrongPass"), _uri);
            await client.OpenAsync();
        }

        [TestMethod]
        public async Task Read()
        {
            var grid = new HaystackGrid()
                .AddColumn("site")
                .AddRow(new HaystackMarker());
            var httpClient = new HttpClientMockBuilder(_uri)
                .WithBasicAuthentication(_basicAuthHash)
                .WithReadAsync("site", grid)
                .Build();
            var client = new HaystackClient(httpClient, new BasicAuthenticator(_user, _pass), _uri);
            await client.OpenAsync();
            await client.ReadAsync("site");
        }

        [TestMethod]
        public void SlashAddedToUri()
        {
            var uri = new Uri("http://localhost:8080/api/demo");
            var client = new HaystackClient(_user, _pass, uri);
            Assert.AreEqual(client.Uri.AbsoluteUri[client.Uri.AbsoluteUri.Length - 1], '/');
        }
    }
}