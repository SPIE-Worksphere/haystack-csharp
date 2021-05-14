using System;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackTimeZone")]
    public class HTimeZone
    {
        public HTimeZone(HaystackTimeZone source)
        {
            Source = source;
        }
        public HaystackTimeZone Source { get; }
        public static HTimeZone make(string name, bool bChecked) => M.Checked(() => M.Map(new HaystackTimeZone(name)), bChecked);
        public static HTimeZone make(TimeZoneInfo dntzi, bool bChecked) => M.Checked(() => M.Map(new HaystackTimeZone(dntzi)), bChecked);
        public bool hequals(object that) => that != null && that is HTimeZone tz && Source.Equals(M.Map(tz));
        public TimeZoneInfo dntz => Source.TimeZoneInfo;
        public static HTimeZone UTC => M.Map(HaystackTimeZone.UTC);
        public static HTimeZone REL => M.Map(HaystackTimeZone.REL);
        public static HTimeZone Default => M.Map(new HaystackTimeZone(TimeZoneInfo.Local.Id));
        public override string ToString() => Source.Name;
    }
}