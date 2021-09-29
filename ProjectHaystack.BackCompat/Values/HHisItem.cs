using System;
using System.Linq;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackHistoryItem")]
    public class HHisItem : HDict
    {
        public HHisItem(HaystackHistoryItem source)
            : base(ToDictionary(source))
        {
            Source = source;
        }

        public HaystackHistoryItem Source { get; }
        public HDateTime TimeStamp => M.Map(Source.TimeStamp);
        public HVal hsVal => M.Map(Source.Value);
        public int hsize() { return 2; }
        public static HHisItem[] gridToItems(HGrid grid) => HaystackHistoryItem.ReadGrid(M.Map(grid)).Select(M.Map).ToArray();
        public static HHisItem make(HDateTime ts, HVal val) => M.Map(new HaystackHistoryItem(M.Map(ts), M.Map(val)));
        public override HVal get(string name, bool bchecked)
        {
            if (name.CompareTo("ts") == 0) return TimeStamp;
            if (name.CompareTo("val") == 0) return hsVal;
            if (!bchecked) return null;
            throw new HaystackUnknownNameException("Name not known: " + name);
        }

        private static HaystackDictionary ToDictionary(HaystackHistoryItem source)
        {
            var dict = new HaystackDictionary();
            dict.Add("ts", source.TimeStamp);
            dict.Add("val", source.Value);
            return dict;
        }
    }
}