//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    /**
     * HNA is the singleton value used to indicate not available.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HNA : HVal
    {
        // Singleton value 
        public static HNA VAL = new HNA();

        private HNA() { }

        // Hash code 
        public int hashCode() { return 0x6e61; }

        // Equals is based on reference 
        public override bool hequals(object that) { return this == that; }

        // Encode as "na" 
        public override string ToString() { return "na"; }

        // Encode as "z:" 
        public override string toJson() { return "z:"; }

        // Encode as "NA" 
        public override string toZinc() { return "NA"; }

    }
}
