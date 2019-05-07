//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack.Auth;
using ProjectHaystack.Client;
using ProjectHaystackTest.Helpers;

namespace ProjectHaystackTest.Client.Tests
{
  [TestClass]
  public class AsyncClientTest
  {
    private static Uri _uri = new Uri("http://localhost:8080/api/demo/");
    private const string _user = "User";
    private const string _pass = "Pass";
    private const string _oneSiteZinc = @"ver:""3.0""
id,tz,occupiedEnd,yearBuilt,dis,regionRef,geoAddr,geoCoord,geoStreet,area,geoState,geoPostalCode,store,geoCity,site,phone,metro,occupiedStart,primaryFunction,weatherRef,geoCountry,storeNum,mod
@p:demo:r:2404ff8d-0e720610 ""Carytown"",""New_York"",20:00:00,1996,""Carytown"",@p:demo:r:2404ff8d-0fe141e8 ""Richmond"",""3504 W Cary St, Richmond, VA"",C(37.555385,-77.486903),""3504 W Cary St"",3149ft²,""VA"",""23221"",M,""Richmond"",M,""804.552.2222"",""Richmond"",10:00:00,""Retail Store"",@p:demo:r:2404ff8d-828f0032 ""Richmond, VA"",""US"",1,2019-02-24T07:09:02.125Z";

    [TestMethod]
    public async Task LoginTest()
    {
      var client = new HAsyncClient(_uri,
        new AsyncAuthClientContext(_uri, _user, _pass)
        {
          ServerCallAsync = new MockServerCallBuilder().Build(),
        });
      await client.OpenAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(AuthException))]
    public async Task BadUserTest()
    {
      var client = new HAsyncClient(_uri,
        new AsyncAuthClientContext(_uri, "baduser", _pass)
        {
          ServerCallAsync = new MockServerCallBuilder().WithFailingLogin().Build(),
        });
      await client.OpenAsync();
    }

    [TestMethod]
    [ExpectedException(typeof(AuthException))]
    public async Task BadPassTest()
    {
      var client = new HAsyncClient(_uri,
        new AsyncAuthClientContext(_uri, _user, "badpass")
        {
          ServerCallAsync = new MockServerCallBuilder().WithFailingLogin().Build(),
        });
      await client.OpenAsync();
    }

    [TestMethod]
    public async Task Read()
    {
      var client = new HAsyncClient(_uri,
        new AsyncAuthClientContext(_uri, _user, _pass)
        {
          ServerCallAsync = new MockServerCallBuilder()
            .WithReadResponse(_oneSiteZinc)
            .Build(),
        });
      await client.OpenAsync();
      await client.readAsync("site", true);
    }

    [TestMethod]
    [ExpectedException(typeof(WebException))]
    public async Task ReadWithoutOpen()
    {
      var client = new HAsyncClient(_uri,
        new AsyncAuthClientContext(_uri, _user, _pass)
        {
          ServerCallAsync = new MockServerCallBuilder()
            .WithReadResponse(_oneSiteZinc)
            .Build(),
        });
      await client.readAsync("site", true);
    }

    [TestMethod]
    public void SlashAddedToUri()
    {
      var uri = new Uri("http://localhost:8080/api/demo");
      var client = new HAsyncClient(uri, _user, _pass);
      Assert.AreEqual(client.Uri.AbsoluteUri[client.Uri.AbsoluteUri.Length - 1], '/');
    }
  }
}