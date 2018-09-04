//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    public abstract class HVal : IComparable
    {
        // Constructor
        public HVal() { }

        // string formt in zinc human readable format
        public override string ToString() { return toZinc(); }

        // Encode the value to zinc format
        public abstract string toZinc();

        // Encode the value to JSON string
        public abstract string toJson();

        // Hash code is value based - is a item from a java hashmap it is not 
        //   readily required to implement the haystack functionality in .NET
        //public abstract int hashCode();

        // Equality is value based
        // .NET Framework has a Equals defintion in the base object class
        // defining a equals is dangerous and could easily get confused
        // For .NET added a h to make it distinguishable
        public abstract bool hequals(object obj);

        // IComparable
        public virtual int CompareTo(object obj)
        {
            return ToString().CompareTo(obj.ToString());
        }
    }
}
