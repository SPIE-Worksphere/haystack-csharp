using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HaystackDictionaryTests
    {
        [TestMethod]
        public void TestEmpty()
        {
            // Instance Empty
            var tags = new HaystackDictionary();
            Assert.AreEqual(tags, new HaystackDictionary());

            // size
            Assert.AreEqual(tags.Count, 0);
            Assert.IsTrue(tags.IsEmpty());

            // missing tag
            Assert.IsFalse(tags.ContainsKey("foo"));
        }

        [TestMethod]
        [ExpectedException(typeof(HaystackUnknownNameException))]
        public void TestCheckedImplicitMissing()
        {
            var tags = new HaystackDictionary();
            tags.Get("foo");
        }

        [TestMethod]
        public void TestBasics()
        {
            var tags = new HaystackDictionary();
            tags.Add("id", new HaystackReference("aaaa-bbbb"));
            tags.Add("site", new HaystackMarker());
            tags.Add("geoAddr", new HaystackString("Richmond, Va"));
            tags.Add("area", new HaystackNumber(1200, "ft"));
            tags.Add("date", new HaystackDate(2000, 12, 3));
            tags.Add("null", null);

            // size
            Assert.AreEqual(5, tags.Count);
            Assert.IsFalse(tags.IsEmpty());

            // configured tags
            Assert.IsTrue(tags.Get("id").Equals(new HaystackReference("aaaa-bbbb")));
            Assert.IsTrue(tags.Get("site").Equals(new HaystackMarker()));
            Assert.IsTrue(tags.Get("geoAddr").Equals(new HaystackString("Richmond, Va")));
            Assert.IsTrue(tags.Get("area").Equals(new HaystackNumber(1200, "ft")));
            Assert.IsTrue(tags.Get("date").Equals(new HaystackDate(2000, 12, 3)));
            Assert.ThrowsException<HaystackUnknownNameException>(() => tags.Get("null"));

            // missing tag
            Assert.IsFalse(tags.ContainsKey("foo"));
            Assert.ThrowsException<HaystackUnknownNameException>(() => tags.Get("foo"));
        }

        [TestMethod]
        public void TestEquality()
        {
            var a = new HaystackDictionary();
            a.Add("x", new HaystackMarker());
            Assert.IsTrue(a.Equals(new HaystackDictionary().AddMarker("x")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddNumber("x", 3)));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddMarker("y")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddMarker("x").AddMarker("y")));

            a = new HaystackDictionary().AddMarker("x").AddString("y", "str");
            Assert.IsTrue(a.Equals(new HaystackDictionary().AddMarker("x").AddString("y", "str")));
            Assert.IsTrue(a.Equals(new HaystackDictionary().AddString("y", "str").AddMarker("x")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddString("x", "str").AddString("y", "str")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddMarker("x").AddString("y", "strx")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddString("y", "str")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddMarker("x")));
            Assert.IsFalse(a.Equals(new HaystackDictionary().AddMarker("x").AddString("yy", "str")));

            a = new HaystackDictionary().AddValue("x", null);
            Assert.IsTrue(a.Equals(new HaystackDictionary().AddValue("x", null)));
            Assert.IsTrue(a.Equals(new HaystackDictionary().AddValue("foo", null).AddValue("bar", null)));
            Assert.IsTrue(a.Equals(new HaystackDictionary()));
        }

        [TestMethod]
        public void TestDisplay()
        {
            Assert.AreEqual("a", new HaystackDictionary().AddValue("id", new HaystackReference("a")).Display);
            Assert.AreEqual("b", new HaystackDictionary().AddValue("id", new HaystackReference("a", "b")).Display);
            Assert.AreEqual("d", new HaystackDictionary().AddValue("id", new HaystackReference("a")).AddString("dis", "d").Display);
        }

        [TestMethod]
        public void TestDef()
        {
            Assert.AreEqual(new HaystackDictionary().AddValue("def", new HaystackDefinition("^a")).Get<HaystackDefinition>("def").Value, "^a");
        }

        [TestMethod]
        public void TestToArray()
        {
            HaystackDictionary a = new HaystackDictionary().AddMarker("x").AddString("y", "str");
            var array = a.ToArray();
            Assert.AreEqual(2, array.Length);
            Assert.AreEqual("x", array[0].Key);
            Assert.AreEqual(new HaystackMarker(), array[0].Value);
            Assert.AreEqual("y", array[1].Key);
            Assert.AreEqual(new HaystackString("str"), array[1].Value);
        }

        [TestMethod]
        public void TestAdd()
        {
            HaystackDictionary dict = new HaystackDictionary().AddMarker("x").AddMarker("y");
            dict.Add("z", new HaystackString("z"));
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(new HaystackMarker(), dict["x"]);
            Assert.AreEqual(new HaystackMarker(), dict["y"]);
            Assert.AreEqual(new HaystackString("z"), dict["z"]);
        }

        [TestMethod]
        public void TestRemove()
        {
            HaystackDictionary dict = new HaystackDictionary().AddMarker("x").AddString("y", "str");
            dict.Remove("y");
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(new HaystackMarker(), dict["x"]);
        }
    }
}