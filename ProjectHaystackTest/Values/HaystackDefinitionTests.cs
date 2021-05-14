using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackDefinitionTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackDefinition("^foo").Equals(new HaystackDefinition("^foo")));
            Assert.IsFalse(new HaystackDefinition("^foo").Equals(new HaystackDefinition("^Foo")));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadDefConstruction()
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
            {
                new HaystackDefinition(strID);
            }
        }
    }
}