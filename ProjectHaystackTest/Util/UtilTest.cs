//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;

namespace ProjectHaystackTest.Util
{
  using WebUtil = ProjectHaystack.Util.WebUtil;
  using Base64 = ProjectHaystack.Util.Base64;
  using Pbkdf2 = ProjectHaystack.Util.Pbkdf2;

  [TestClass]
  public class UtilTest
  {
    [TestMethod]
    public void IsTokenTest()
    {
      Assert.AreEqual(WebUtil.IsToken(""), false);
      Assert.AreEqual(WebUtil.IsToken("x"), true);
      Assert.AreEqual(WebUtil.IsToken("x y"), false);
      Assert.AreEqual(WebUtil.IsToken("5a-3dd_33*&^%22!~"), true);
      Assert.AreEqual(WebUtil.IsToken("(foo)"), false);
      Assert.AreEqual(WebUtil.IsToken("foo;bar"), false);
      Assert.AreEqual(WebUtil.IsToken("base64+/"), false);
    }

    private static String RandomString()
    {
      Guid g = Guid.NewGuid();
      string GuidString = Convert.ToBase64String(g.ToByteArray());
      GuidString = GuidString.Replace("=", "");
      GuidString = GuidString.Replace("+", "");
      return GuidString;
    }

    [TestMethod]
    public void Base64Test()
    {
      for (int i = 0; i < 1000; i++)
      {
        string s1 = RandomString();
        string enc = Base64.STANDARD.EncodeUtf8(s1);
        string s2 = Base64.STANDARD.decodeUTF8(enc);
        Assert.AreEqual(s1, s2);

        enc = Base64.STANDARD.Encode(s1);
        s2 = Base64.STANDARD.Decode(enc);
        Assert.AreEqual(s1, s2);

        enc = Base64.URI.EncodeUtf8(s1);
        s2 = Base64.URI.decodeUTF8(enc);
        Assert.AreEqual(s1, s2);

        enc = Base64.URI.Encode(s1);
        s2 = Base64.URI.Decode(enc);
        Assert.AreEqual(s1, s2);
      }
    }


    public void DoPbkTest(string password, string salt, int iterations, int dkLen, string expected)
    {
      using (var hmac = new HMACSHA256())
      {
        var mine = new Pbkdf2(hmac, System.Text.Encoding.UTF8.GetBytes(password), System.Text.Encoding.UTF8.GetBytes(salt), iterations);
        sbyte[] result = mine.GetBytes(dkLen);
        byte[] usResult = (byte[])(Array)result;
        String hex = BitConverter.ToString(usResult);
        Assert.AreEqual(hex, expected);
      }
    }

    [TestMethod]
    public void PbkTest()
    {
      DoPbkTest("password", "salt", 1, 32, "12-0F-B6-CF-FC-F8-B3-2C-43-E7-22-52-56-C4-F8-37-A8-65-48-C9-2C-CC-35-48-08-05-98-7C-B7-0B-E1-7B");
      DoPbkTest("password", "salt", 2, 32, "AE-4D-0C-95-AF-6B-46-D3-2D-0A-DF-F9-28-F0-6D-D0-2A-30-3F-8E-F3-C2-51-DF-D6-E2-D8-5A-95-47-4C-43");
      DoPbkTest("password", "salt", 4096, 32, "C5-E4-78-D5-92-88-C8-41-AA-53-0D-B6-84-5C-4C-8D-96-28-93-A0-01-CE-4E-11-A4-96-38-73-AA-98-13-4A");
    }

    
  }
}
