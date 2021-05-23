//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectHaystack;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystackTest
{
    [TestClass]
    public abstract class HValTest : HaystackTest
    {
        protected void verifyZinc(HVal val, string s)
        {
            Assert.AreEqual(val.toZinc(), s);
            Assert.IsTrue(read(s).hequals(val));
        }

        protected HVal read(string s)
        {
            return M.Map(ZincReader.ReadValue(s));
        }
    }
}