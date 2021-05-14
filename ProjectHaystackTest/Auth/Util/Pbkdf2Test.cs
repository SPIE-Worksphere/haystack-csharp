using System;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectHaystackTest.Util
{
    using Pbkdf2 = ProjectHaystack.Util.Pbkdf2;

    [TestClass]
    public class Pbkdf2Test
    {
        public void DoPbkTest(string password, string salt, int iterations, int dkLen, string expected)
        {
            using (var hmac = new HMACSHA256())
            {
                var mine = new Pbkdf2(hmac, System.Text.Encoding.UTF8.GetBytes(password), System.Text.Encoding.UTF8.GetBytes(salt), iterations);
                var result = mine.GetBytes(dkLen);
                var usResult = (byte[])(Array)result;
                var hex = BitConverter.ToString(usResult);
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