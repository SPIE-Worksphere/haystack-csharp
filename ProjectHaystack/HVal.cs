//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;

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

        // Equality is value based
        // .NET Framework has a Equals defintion in the base object class
        // defining a equals is dangerous and could easily get confused
        // For .NET added a h to make it distinguishable
        public bool hequals(object obj) => Equals(obj);
        public int hashCode() => GetHashCode();

        // IComparable
        public virtual int CompareTo(object obj)
        {
            return ToString().CompareTo(obj.ToString());
        }
    }
}