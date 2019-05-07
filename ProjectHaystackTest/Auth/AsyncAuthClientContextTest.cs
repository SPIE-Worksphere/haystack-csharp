//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack.Auth;

namespace ProjectHaystackTest.Auth
{
    [TestClass]
    public class AsyncAuthClientContextTest
    {
        private const string _user = "User";
        private const string _pass = "Pass";

        [TestMethod]
        public void SlashAddedToUri()
        {
            var uri = new Uri("http://localhost:8080/api/demo");
            var context = new AsyncAuthClientContext(uri, _user, _pass);
            Assert.AreEqual(context.Uri.AbsoluteUri[context.Uri.AbsoluteUri.Length - 1], '/');
        }
    }
}
