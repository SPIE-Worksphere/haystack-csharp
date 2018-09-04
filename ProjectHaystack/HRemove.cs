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
     * HRemove is the singleton value used to indicate a tag remove.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HRemove : HVal
    {
        // Singleton value 
        public static HRemove VAL = new HRemove();

        private HRemove() { }

        // Hash code 
        public int hashCode() { return 0x8ab3; }

        // Equals is based on reference 
        public override bool hequals(object that) { return this == that; }

        // Encode as "remove" 
        public override string ToString() { return "remove"; }

        // Encode as "x:" 
        public override string toJson() { return "x:"; }

        // Encode as "R" 
        public override string toZinc() { return "R"; }
    }
}
