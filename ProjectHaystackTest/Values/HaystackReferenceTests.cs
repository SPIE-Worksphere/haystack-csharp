using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackReferenceTests
    {
        [TestMethod]
        public void TestEquality()
        {
            Assert.IsTrue(new HaystackReference("foo").Equals(new HaystackReference("foo")));
            Assert.IsTrue(new HaystackReference("foo").Equals(new HaystackReference("foo", "Foo")));
            Assert.IsFalse(new HaystackReference("foo").Equals(new HaystackReference("Foo")));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadRefConstruction()
        {
            string[] badRefs = new string[]
            {
                "@a",
                "a b",
                "a\n",
                "@"
            };
            foreach (string strID in badRefs)
            {
                new HaystackReference(strID);
            }
        }
    }
}