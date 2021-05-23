//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    [TestClass]
    public class HBinTest : HValTest
    {
        [TestMethod]
        public void testEquality()
        {
            HBin bin1 = HBin.make("text/plain");
            HBin bin2 = HBin.make("text/plain");
            HBin bin3 = HBin.make("text/xml");
            // Can't user AreEqual or AreNotEqual as these are objects and that is doing 
            //   a reference check not a logical value test because HBin does not implement
            //   IComparable Interface - Haystack equals determines equality for this library purpose
            //e.g this will fail = Assert.AreEqual(HBin.make("text/plain"), HBin.make("text/plain"));
            Assert.IsTrue(bin1.hequals(bin2));
            Assert.IsFalse(bin1.hequals(bin3));
        }
        // TODO:FIXIT
        //    // encoding
        //    verifyZinc(HBin.make("text/plain"), "Bin(\"text/plain\")");
        //    verifyZinc(HBin.make("text/plain; charset=utf-8"), "Bin(\"text/plain; charset=utf-8\")");
        //
        //    // verify bad bins are caught on encoding
        //    try { HBin.make("text/plain; f()").toZinc(); fail(); } catch (Exception e) { verifyException(e); }
        //    try { read("Bin()"); fail(); } catch (Exception e) { verifyException(e); }
        //    try { read("Bin(\"text\")"); fail(); } catch (Exception e) { verifyException(e); }
    }
}