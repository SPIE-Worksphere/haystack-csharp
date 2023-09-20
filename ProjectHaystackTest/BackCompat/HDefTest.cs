using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HDefTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HDef.make("^foo").hequals(HDef.make("^foo")));
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HDef.make("^testDef"), "^testDef");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void testBadDefConstruction()
        {
            string[] badDefs = new string[]
            {
                "^@a",
                "^a b",
                "^a\n",
                "^@",
                "a",
                "bcd",
            };
            foreach (string strID in badDefs)
                HDef.make(strID);
        }
    }
}