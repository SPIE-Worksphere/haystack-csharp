using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackListTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            Assert.IsTrue(new HaystackList().Equals(new HaystackList(new List<HaystackValue>())));
            Assert.IsTrue(new HaystackList().Equals(new HaystackList(new HaystackValue[0])));
            Assert.AreEqual(new HaystackList().Count, 0);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new HaystackList()[0]);
        }

        [TestMethod]
        public void TestBasics()
        {
            var hrefTest = new HaystackReference("a");
            var str = new HaystackString("string");
            List<HaystackValue> items = new List<HaystackValue>();
            items.Add(hrefTest);
            items.Add(str);

            var list = new HaystackList(items);
            Assert.AreEqual(list.Count, 2);
            Assert.AreEqual(list[0], hrefTest);
            Assert.AreEqual(list[1], str);
        }
    }
}