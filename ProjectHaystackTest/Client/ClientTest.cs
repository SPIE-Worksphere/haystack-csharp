//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack.Auth;
using ProjectHaystack.Client;

namespace ProjectHaystackTest.Client.Tests
{
  [TestClass]
  public class ClientTest
  {
    private string uri = "http://localhost:8080/api/demo";
    private string user = "User";
    private string pass = "Pass";

    private HClient client;

    [TestInitialize]
    public void Setup()
    {
      this.client = HClient.Open(uri, user, pass);
    }

    [TestMethod]
    [ExpectedException(typeof(AuthException))]
    public void BadUserTest()
    {
      HClient.Open(uri, "baduser", pass);
    }

    [TestMethod]
    [ExpectedException(typeof(AuthException))]
    public void BadPassTest()
    {
      HClient.Open(uri, user, "badpass");
    }
  }
}
