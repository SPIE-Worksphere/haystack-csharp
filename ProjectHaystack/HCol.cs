//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//

namespace ProjectHaystack
{
    public class HCol
    {
        // Constructor
        public HCol(int iIndex, string name, HDict meta)
        {
            Index = iIndex;
            Name = name;
            this.meta = meta;
        }

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        // Return programatic name of column 
        public string Name { get; }

        // Return display name of column which is meta.dis or name
        public string dis()
        {
            HVal dis = meta.get("dis", false);
            if (dis is HStr) return ((HStr)dis).Value;
            return Name;
        }

        // Column meta-data tags 
        public HDict meta { get; }

        // Access Index 
        public int Index { get; }

        //////////////////////////////////////////////////////////////////////////
        // Identity
        //////////////////////////////////////////////////////////////////////////

        // Hash code is not needed for .NET - see notes in HDict

        // Equality is name and meta 
        public bool hequals(object that)
        {
            if (!(that is HCol)) return false;
            HCol x = (HCol)that;
            return ((Name.Equals(x.Name)) && (meta.hequals(x.meta)));
        }
    }
}