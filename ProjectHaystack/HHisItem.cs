//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;

namespace ProjectHaystack
{
    /**
     * HHisItem is used to model a timestamp/value pair
     *
     * @see <a href='http://project-haystack.org/doc/Ops#hisRead'>Project Haystack</a>
     */
    public class HHisItem : HDict
    {
        // Access
        public HDateTime TimeStamp { get; }
        public HVal hsVal { get; }
        public int hsize() { return 2; } // size is two - timestamp and value

        // Private constructor 
        private HHisItem(HDateTime ts, HVal val) : base(new Dictionary<string, HVal>(11))
        {
            TimeStamp = ts;
            hsVal = val;
        }

        // Map HGrid to HHisItem[].  Grid must have ts and val columns. 
        public static HHisItem[] gridToItems(HGrid grid)
        {
            HCol ts = grid.col("ts");
            HCol val = grid.col("val");
            HHisItem[] items = new HHisItem[grid.numRows];
            for (int i = 0; i < items.Length; ++i)
            {
                HRow row = grid.row(i);
                // Timestamp can't be NULL but val can
                items[i] = new HHisItem((HDateTime)row.get(ts, true), row.get(val, false));
            }
            return items;
        }

        // Construct from timestamp, value 
        public static HHisItem make(HDateTime ts, HVal val)
        {
            if (ts == null || val == null) throw new ArgumentException("ts or val is null");
            return new HHisItem(ts, val);
        }

        public override HVal get(string name, bool bchecked)
        {
            if (name.CompareTo("ts") == 0) return TimeStamp;
            if (name.CompareTo("val") == 0) return hsVal;
            if (!bchecked) return null;
            throw new UnknownNameException("Name not known: " + name);
        }
    }
}