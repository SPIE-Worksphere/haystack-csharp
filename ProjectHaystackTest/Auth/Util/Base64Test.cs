using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHaystackTest.Util
{
    using Base64 = ProjectHaystack.Util.Base64;

    [TestClass]
    public class Base64Test
    {
        private static string RandomString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }

        [TestMethod]
        public void StandardEncodeDecode()
        {
            for (var i = 0; i < 1000; i++)
            {
                var s1 = RandomString();
                var enc = Base64.STANDARD.Encode(s1);
                var s2 = Base64.STANDARD.Decode(enc);
                Assert.AreEqual(s1, s2);
            }
        }

        [TestMethod]
        public void StandardUtf8EncodeDecode()
        {
            for (int i = 0; i < 1000; i++)
            {
                var s1 = RandomString();
                var enc = Base64.STANDARD.EncodeUtf8(s1);
                var s2 = Base64.STANDARD.decodeUTF8(enc);
                Assert.AreEqual(s1, s2);
            }
        }

        [TestMethod]
        public void UriEncodeDecode()
        {
            for (int i = 0; i < 1000; i++)
            {
                var s1 = RandomString();
                var enc = Base64.URI.Encode(s1);
                var s2 = Base64.URI.Decode(enc);
                Assert.AreEqual(s1, s2);
            }
        }

        [TestMethod]
        public void UriUtf8EncodeDecode()
        {
            for (int i = 0; i < 1000; i++)
            {
                var s1 = RandomString();
                var enc = Base64.URI.EncodeUtf8(s1);
                var s2 = Base64.URI.decodeUTF8(enc);
                Assert.AreEqual(s1, s2);
            }
        }
    }
}