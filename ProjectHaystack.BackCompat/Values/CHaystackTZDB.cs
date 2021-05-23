using System;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackTimeZoneDatabase")]
    public class CHaystackTZDB
    {
        public static string[] tzdb = HaystackTimeZoneDatabase.TimeZones;
    }
}