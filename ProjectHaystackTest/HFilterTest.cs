//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    // Interfaces need to work differently in .NET - this is not a callback delegate scenario in reality - it is really just
    //   pass an object that implements the interface. 
    public class dbPather : HFilter.IPather
    {
        private Dictionary<string, HDict> m_map;
        // Access
        public Dictionary<string, HDict> Map { get { return m_map; } }
        public dbPather(Dictionary<string, HDict> mp)
        {
            m_map = mp;
        }
        public HDict find(string id)
        {
            return m_map[id];
        }
    }
    [TestClass]
    public class HFilterTest : HaystackTest
    {
        [TestMethod]
        public void testIdentity()
        {
            Assert.IsTrue(HFilter.has("a").hequals(HFilter.has("a")));
            Assert.IsFalse(HFilter.has("a").hequals(HFilter.has("b")));
        }

        [TestMethod]
        public void testBasics()
        {
            verifyParse("x", HFilter.has("x"));
            verifyParse("foo", HFilter.has("foo"));
            verifyParse("fooBar", HFilter.has("fooBar"));
            verifyParse("foo7Bar", HFilter.has("foo7Bar"));
            verifyParse("foo_bar->a", HFilter.has("foo_bar->a"));
            verifyParse("a->b->c", HFilter.has("a->b->c"));
            verifyParse("not foo", HFilter.missing("foo"));
        }

        [TestMethod]
        public void testZincOnlyLiteralsDontWork()
        {
            Assert.IsNull(HFilter.make("x==T", false));
            Assert.IsNull(HFilter.make("x==F", false));
            Assert.IsNull(HFilter.make("x==F", false));
        }

        [TestMethod]
        public void testBool()
        {
            verifyParse("x->y==true", HFilter.eq("x->y", HBool.TRUE));
            verifyParse("x->y!=false", HFilter.ne("x->y", HBool.FALSE));
        }

        [TestMethod]
        public void testStr()
        {
            verifyParse("x==\"hi\"", HFilter.eq("x", HStr.make("hi")));
            verifyParse("x!=\"\\\"hi\\\"\"", HFilter.ne("x", HStr.make("\"hi\"")));
            verifyParse("x==\"_\\u00a3_\\n_\"", HFilter.eq("x", HStr.make("_\u00a3_\n_"))); // Was abcd but changed to valid unicode pound
        }

        [TestMethod]
        public void testUri()
        {
            verifyParse("ref==`http://foo/?bar`", HFilter.eq("ref", HUri.make("http://foo/?bar")));
            verifyParse("ref->x==`file name`", HFilter.eq("ref->x", HUri.make("file name")));
            verifyParse("ref == `foo bar`", HFilter.eq("ref", HUri.make("foo bar")));
        }

        [TestMethod]
        public void testInt()
        {
            verifyParse("num < 4", HFilter.lt("num", n(4)));
            verifyParse("num <= -99", HFilter.le("num", n(-99)));
        }

        [TestMethod]
        public void testFloat()
        {
            verifyParse("num < 4.0", HFilter.lt("num", n(4f)));
            verifyParse("num <= -9.6", HFilter.le("num", n(-9.6f)));
            verifyParse("num > 400000", HFilter.gt("num", n(4e5f)));
            verifyParse("num >= 16000", HFilter.ge("num", n(1.6e+4f)));
            verifyParse("num >= 2.16", HFilter.ge("num", n(2.16)));
        }

        [TestMethod]
        public void testUnit()
        {
            verifyParse("dur < 5ns", HFilter.lt("dur", n(5, "ns")));
            verifyParse("dur < 10kg", HFilter.lt("dur", n(10, "kg")));
            verifyParse("dur < -9sec", HFilter.lt("dur", n(-9, "sec")));
            verifyParse("dur < 2.5hr", HFilter.lt("dur", n(2.5, "hr")));
        }

        [TestMethod]
        public void testDateTime()
        {
            verifyParse("foo < 2009-10-30", HFilter.lt("foo", HDate.make("2009-10-30")));
            verifyParse("foo < 08:30:00", HFilter.lt("foo", HTime.make("08:30:00")));
            verifyParse("foo < 13:00:00", HFilter.lt("foo", HTime.make("13:00:00")));
        }

        [TestMethod]
        public void testRef()
        {
            verifyParse("author == @xyz", HFilter.eq("author", HRef.make("xyz")));
            verifyParse("author==@xyz:foo.bar", HFilter.eq("author", HRef.make("xyz:foo.bar")));
        }

        [TestMethod]
        public void testAnd()
        {
            verifyParse("a and b", HFilter.has("a").and(HFilter.has("b")));
            verifyParse("a and b and c == 3", HFilter.has("a").and(HFilter.has("b").and(HFilter.eq("c", n(3)))));
        }

        [TestMethod]
        public void testOr()
        {
            verifyParse("a or b", HFilter.has("a").or(HFilter.has("b")));
            verifyParse("a or b or c == 3", HFilter.has("a").or(HFilter.has("b").or(HFilter.eq("c", n(3)))));
        }

        [TestMethod]
        public void testParens()
        {
            verifyParse("(a)", HFilter.has("a"));
            verifyParse("(a) and (b)", HFilter.has("a").and(HFilter.has("b")));
            verifyParse("( a )  and  ( b ) ", HFilter.has("a").and(HFilter.has("b")));
            verifyParse("(a or b) or (c == 3)", HFilter.has("a").or(HFilter.has("b")).or(HFilter.eq("c", n(3))));
        }

        [TestMethod]
        public void testCombo()
        {
            HFilter isA = HFilter.has("a");
            HFilter isB = HFilter.has("b");
            HFilter isC = HFilter.has("c");
            HFilter isD = HFilter.has("d");
            verifyParse("a and b or c", (isA.and(isB)).or(isC));
            verifyParse("a or b and c", isA.or(isB.and(isC)));
            verifyParse("a and b or c and d", (isA.and(isB)).or(isC.and(isD)));
            verifyParse("(a and (b or c)) and d", isA.and(isB.or(isC)).and(isD));
            verifyParse("(a or (b and c)) or d", isA.or(isB.and(isC)).or(isD));
        }

        void verifyParse(string s, HFilter expected)
        {
            HFilter actual = HFilter.make(s);
            Assert.IsTrue(actual.hequals(expected));
        }

        [TestMethod]
        public void testInclude()
        {
            HDict a = new HDictBuilder()
              .add("dis", "a")
              .add("num", 100)
              .add("foo", 99)
              .add("date", HDate.make(2011, 10, 5))
              .toDict();

            HDict b = new HDictBuilder()
              .add("dis", "b")
              .add("num", 200)
              .add("foo", 88)
              .add("date", HDate.make(2011, 10, 20))
              .add("bar")
              .add("ref", HRef.make("a"))
              .toDict();

            HDict c = new HDictBuilder()
              .add("dis", "c")
              .add("num", 300)
              .add("ref", HRef.make("b"))
              .add("bar")
              .toDict();

            Dictionary<string, HDict> db = new Dictionary<string, HDict>();
            db.Add("a", a);
            db.Add("b", b);
            db.Add("c", c);

            verifyInclude(db, "dis", "a,b,c");
            verifyInclude(db, "dis == \"b\"", "b");
            verifyInclude(db, "dis != \"b\"", "a,c");
            verifyInclude(db, "dis <= \"b\"", "a,b");
            verifyInclude(db, "dis >  \"b\"", "c");
            verifyInclude(db, "num < 200", "a");
            verifyInclude(db, "num <= 200", "a,b");
            verifyInclude(db, "num > 200", "c");
            verifyInclude(db, "num >= 200", "b,c");
            verifyInclude(db, "date", "a,b");
            verifyInclude(db, "date == 2011-10-20", "b");
            verifyInclude(db, "date < 2011-10-10", "a");
            verifyInclude(db, "foo", "a,b");
            verifyInclude(db, "not foo", "c");
            verifyInclude(db, "foo == 88", "b");
            verifyInclude(db, "foo != 88", "a");
            verifyInclude(db, "foo == \"x\"", "");
            verifyInclude(db, "ref", "b,c");
            verifyInclude(db, "ref->dis", "b,c");
            verifyInclude(db, "ref->dis == \"a\"", "b");
            verifyInclude(db, "ref->bar", "c");
            verifyInclude(db, "not ref->bar", "a,b");
            verifyInclude(db, "foo and bar", "b");
            verifyInclude(db, "foo or bar", "a,b,c");
            verifyInclude(db, "(foo and bar) or num==300", "b,c");
            verifyInclude(db, "foo and bar and num==300", "");
        }

        private void verifyInclude(Dictionary<string, HDict> map, string query, string expected)
        {

            dbPather db = new dbPather(map);
            /* old Java style interface implementation the above replaces            
             * db()
             * {
             *    public HDict find(String id) { return map.get(id); }
             * };
             */

            HFilter q = HFilter.make(query);

            string actual = "";
            // I don't like char loops like this but it is not incorrect either for
            //  sake of consistency with Java toolkit I have left it alone
            for (int c = 'a'; c <= 'c'; ++c)
            {
                string id = "" + (char)c;
                if (q.include(db.find(id), db))
                    actual += actual.Length > 0 ? "," + id : id;
            }
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void testPath()
        {
            // single name
            HFilter.Path path = HFilter.Path.make("foo");
            Assert.AreEqual(path.size(), 1);
            Assert.AreEqual(path.get(0), "foo");
            Assert.AreEqual(path.ToString(), "foo");
            Assert.IsTrue(path.hequals(HFilter.Path.make("foo")));

            // two names
            path = HFilter.Path.make("foo->bar");
            Assert.AreEqual(path.size(), 2);
            Assert.AreEqual(path.get(0), "foo");
            Assert.AreEqual(path.get(1), "bar");
            Assert.AreEqual(path.ToString(), "foo->bar");
            Assert.IsTrue(path.hequals(HFilter.Path.make("foo->bar")));

            // three names
            path = HFilter.Path.make("x->y->z");
            Assert.AreEqual(path.size(), 3);
            Assert.AreEqual(path.get(0), "x");
            Assert.AreEqual(path.get(1), "y");
            Assert.AreEqual(path.get(2), "z");
            Assert.AreEqual(path.ToString(), "x->y->z");
            Assert.IsTrue(path.hequals(HFilter.Path.make("x->y->z")));
        }
    }
}