//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 August 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectHaystack;

namespace ProjectHaystackTest
{
    public abstract class HaystackTest
    {
        protected virtual HNum n(long val) { return HNum.make(val); }
        protected virtual HNum n(double val) { return HNum.make(val); }
        protected virtual HNum n(double val, string unit) { return HNum.make(val, unit); }
    }
}
