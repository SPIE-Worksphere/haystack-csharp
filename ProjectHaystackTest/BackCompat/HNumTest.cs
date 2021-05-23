//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HNumTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            Assert.IsTrue(HNum.make(2).hequals(HNum.make(2.0, null)));
            Assert.IsFalse(HNum.make(2).hequals(HNum.make(2, "%")));
            Assert.IsFalse(HNum.make(2, "%").hequals(HNum.make(2)));
            Assert.IsTrue(HNum.make(0).hequals(HNum.make(0.0)));
        }

        [TestMethod]
        public void testCompare()
        {
            Assert.IsTrue(HNum.make(9).compareTo(HNum.make(11)) < 0);
            Assert.IsTrue(HNum.make(-3).compareTo(HNum.make(-4)) > 0);
            Assert.AreEqual(HNum.make(-23).compareTo(HNum.make(-23)), 0);
        }

        [TestMethod]
        public void testZinc()
        {
            verifyZinc(HNum.make(123), "123");
            verifyZinc(HNum.make(123.4, "m/s"), "123.4m/s");
            verifyZinc(HNum.make(9.6, "m/s"), "9.6m/s");
            verifyZinc(HNum.make(-5.2, "\u00b0F"), "-5.2\u00b0F");
            verifyZinc(HNum.make(23, "%"), "23%");
            verifyZinc(HNum.make(2.4e-3, "fl_oz"), "0.0024fl_oz");
            verifyZinc(HNum.make(2.4e5, "$"), "240000$");
            Assert.IsTrue(read("1234.56fl_oz").hequals(HNum.make(1234.56, "fl_oz")));
            Assert.IsTrue(read("0.000028fl_oz").hequals(HNum.make(0.000028, "fl_oz")));

            // specials
            verifyZinc(HNum.make(double.NegativeInfinity), "-INF");
            verifyZinc(HNum.make(double.PositiveInfinity), "INF");
            verifyZinc(HNum.make(double.NaN), "NaN");

            // verify units never serialized for special values
            Assert.AreEqual(HNum.make(double.NaN, "ignore").toZinc(), "NaN");
            Assert.AreEqual(HNum.make(double.PositiveInfinity, "%").toZinc(), "INF");
            Assert.AreEqual(HNum.make(double.NegativeInfinity, "%").toZinc(), "-INF");
        }

        [TestMethod]
        public void verifyUnitNames()
        {
            Assert.IsTrue(HNum.isUnitName(null));
            Assert.IsFalse(HNum.isUnitName(""));
            Assert.IsTrue(HNum.isUnitName("x_z"));
            Assert.IsFalse(HNum.isUnitName("x z"));
        }

        [TestMethod]
        public void testFormatDecimalWithDot()
        {
            string defaultLanguage = CultureInfo.InvariantCulture.ToString();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            verifyZinc(HNum.make(2.4), "2.4");
            Thread.CurrentThread.CurrentCulture = new CultureInfo(defaultLanguage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void testBadUnitConstruction()
        {
            string[] badunitNames = new string[]
            {
                "foo bar",
                "foo,bar"
            };
            foreach (string curUnit in badunitNames)
                HNum.make(123.4, curUnit);
        }
    }
}
