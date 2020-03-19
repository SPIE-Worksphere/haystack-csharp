//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HDictTest : HValTest
    {
        [TestMethod]
        public void testEmpty()
        {
            // Instance Empty
            HDict tags = new HDictBuilder().toDict();
            Assert.IsTrue(tags == HDict.Empty);
            Assert.AreEqual(tags, HDict.Empty);

            // size
            Assert.AreEqual(tags.size(), 0);
            Assert.IsTrue(tags.isEmpty());

            // missing tag
            Assert.IsFalse(tags.has("foo"));
            Assert.IsTrue(tags.missing("foo"));
            Assert.IsNull(tags.get("foo", false));
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownNameException))]
        public void testCheckedImplicitMissing()
        {
            HDict tags = new HDictBuilder().toDict();
            tags.get("foo");
        }
        
        [TestMethod]
        [ExpectedException(typeof(UnknownNameException))]
        public void testCheckedExplicitMissing()
        {
            HDict tags = new HDictBuilder().toDict();
            tags.get("foo", true);
        }

        [TestMethod]
        public void testIsTagName()
        {
            Assert.IsFalse(HDict.isTagName(""));
            Assert.IsFalse(HDict.isTagName("A"));
            Assert.IsFalse(HDict.isTagName(" "));
            Assert.IsTrue(HDict.isTagName("a"));
            Assert.IsTrue(HDict.isTagName("a_B_19"));
            Assert.IsFalse(HDict.isTagName("a b"));
            Assert.IsFalse(HDict.isTagName("a\u0128"));
            Assert.IsFalse(HDict.isTagName("a\u0129x"));
            Assert.IsFalse(HDict.isTagName("a\uabcdx"));
            Assert.IsTrue(HDict.isTagName("^tag"));
        }

        [TestMethod]
        public void testBasics()
        {
            HDict tags = new HDictBuilder()
              .add("id", HRef.make("aaaa-bbbb"))
              .add("site")
              .add("geoAddr", "Richmond, Va")
              .add("area", 1200, "ft")
              .add("date", HDate.make(2000, 12, 3))
              .add("null", (HVal)null)
              .toDict();

            // size
            Assert.AreEqual(tags.size(), 6);
            Assert.IsFalse(tags.isEmpty());

            // configured tags
            Assert.IsTrue(tags.get("id").hequals(HRef.make("aaaa-bbbb")));
            Assert.IsTrue(tags.get("site").hequals(HMarker.VAL));
            Assert.IsTrue(tags.get("geoAddr").hequals(HStr.make("Richmond, Va")));
            Assert.IsTrue(tags.get("area").hequals(HNum.make(1200, "ft")));
            Assert.IsTrue(tags.get("date").hequals(HDate.make(2000, 12, 3)));
            Assert.AreEqual(tags.get("null", false), null);
            try
            {
                tags.get("null");
                Assert.Fail();
            }
            catch (UnknownNameException)
            {
                Assert.IsTrue(true);
            }

            // missing tag
            Assert.IsFalse(tags.has("foo"));
            Assert.IsTrue(tags.missing("foo"));
            Assert.IsNull(tags.get("foo", false));
            try { tags.get("foo"); Assert.Fail(); } catch (UnknownNameException) { Assert.IsTrue(true); }
            try { tags.get("foo", true); Assert.Fail(); } catch (UnknownNameException) { Assert.IsTrue(true); }
        }

        [TestMethod]
        public void testEquality()
        {
            HDict a = new HDictBuilder().add("x").toDict();
            Assert.IsTrue(a.hequals(new HDictBuilder().add("x").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("x", 3).toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("y").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("x").add("y").toDict()));

            a = new HDictBuilder().add("x").add("y", "str").toDict();
            Assert.IsTrue(a.hequals(new HDictBuilder().add("x").add("y", "str").toDict()));
            Assert.IsTrue(a.hequals(new HDictBuilder().add("y", "str").add("x").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("x", "str").add("y", "str").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("x").add("y", "strx").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("y", "str").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("x").toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("x").add("yy", "str").toDict()));

            a = new HDictBuilder().add("x", (HVal)null).toDict();
            Assert.IsTrue(a.hequals(new HDictBuilder().add("x", (HVal)null).toDict()));
            Assert.IsFalse(a.hequals(new HDictBuilder().add("foo", (HVal)null).add("bar", (HVal)null).toDict()));
            Assert.IsFalse(a.hequals(HDict.Empty));
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(
              HDict.Empty,
              "{}");
            verifyZinc(
              new HDictBuilder().add("foo_12").toDict(),
              "{foo_12}");
            verifyZinc(
              new HDictBuilder().add("fooBar", 123, "ft").toDict(),
              "{fooBar:123ft}");
            verifyZinc(
              new HDictBuilder().add("dis", "Bob").add("bday", HDate.make(1970, 6, 3)).add("marker").toDict(),
              "{dis:\"Bob\" bday:1970-06-03 marker}");

            // nested dict
            verifyZinc(
              new HDictBuilder().add("auth", HDict.Empty).toDict(),
              "{auth:{}}");
            verifyZinc(
              new HDictBuilder().add("auth",
                new HDictBuilder().add("alg", "scram").add("c", 10000).add("marker").toDict()
              ).toDict(),
              "{auth:{alg:\"scram\" c:10000 marker}}");

            // nested list
            verifyZinc(
              new HDictBuilder().add("arr", HList.make(new HVal[] { HNum.make(1.0), HNum.make(2), HNum.make(3) }))
                .add("x").toDict(),
              "{arr:[1,2,3] x}"); // Was "{arr:[1.0,2,3] x}" - double in .NET will not recognise the difference between 1.0 and 1
        }

        [TestMethod]
        public void testDis()
        {
            Assert.AreEqual(new HDictBuilder().add("id", HRef.make("a")).toDict().dis(), "a");
            Assert.AreEqual(new HDictBuilder().add("id", HRef.make("a", "b")).toDict().dis(), "b");
            Assert.AreEqual(new HDictBuilder().add("id", HRef.make("a")).add("dis", "d").toDict().dis(), "d");
        }

        [TestMethod]
        public void testDef()
        {
            HDict def = new HDictBuilder()
              .add("id", HRef.make("aaaa-bbbb"))
              .add("def", "^defType")
              .add("^defType", "Some description")
              .add("mod", HDate.make(2000, 12, 3))
              .toDict();

            Assert.AreEqual(new HDictBuilder().add("id", HRef.make("a")).toDict().dis(), "a");
            Assert.AreEqual(new HDictBuilder().add("id", HRef.make("a", "b")).toDict().dis(), "b");
            Assert.AreEqual(new HDictBuilder().add("id", HRef.make("a")).add("dis", "d").toDict().dis(), "d");
        }
    }
}