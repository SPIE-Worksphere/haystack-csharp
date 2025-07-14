using System;
using System.Linq;
using TimeZoneConverter;
#if NET6_0_OR_GREATER
#else
using TimeZoneConverter;
#endif

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack time zone information.
    /// </summary>
    /// <remarks>
    /// Handles mapping between Haystack timezone names and dotnet timezones.
    /// </remarks>
    public class HaystackTimeZone
    {
        public HaystackTimeZone(string name, TimeZoneInfo timeZoneInfo)
        {
            Name = name;
            TimeZoneInfo = timeZoneInfo;
        }

        public HaystackTimeZone(string name)
        {
            Name = name;
            TimeZoneInfo = LocateTimeZoneByName(name);
        }

        public HaystackTimeZone(TimeZoneInfo timeZoneInfo)
        {
            TimeZoneInfo = timeZoneInfo;
            Name = LocateTimeZoneName(timeZoneInfo);
        }

        public string Name { get; }
        public TimeZoneInfo TimeZoneInfo { get; }

        public static HaystackTimeZone UTC => new HaystackTimeZone("UTC");

        public static HaystackTimeZone REL => new HaystackTimeZone("Rel");

        public static HaystackTimeZone Default => new HaystackTimeZone(TimeZoneInfo.Local.Id);

        public override int GetHashCode() => Name.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackTimeZone timeZone && timeZone.Name.Equals(Name);

        private static TimeZoneInfo LocateTimeZoneByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value must not be empty", nameof(name));
            }

            string nameToSearch = name?.ToUpper().Trim();

            // For "Rel" timezone, use "GMT"
            if (nameToSearch == "REL")
            {
                nameToSearch = "GMT";
            }

            TimeZoneInfo tziFound = null;
#if NET6_0_OR_GREATER
            try
            {
                tziFound = TimeZoneInfo.FindSystemTimeZoneById(nameToSearch);
            }
            catch (TimeZoneNotFoundException)
            {
                tziFound = TimeZoneInfo.GetSystemTimeZones()
                    .Where(tz => tz.StandardName.ToUpper().Contains(nameToSearch))
                    .OrderBy(tz => tz.StandardName.Length)
                    .FirstOrDefault();
                if (tziFound == null)
                {
                    tziFound = TimeZoneInfo.GetSystemTimeZones()
                        .Where(tz => tz.DisplayName.ToUpper().Contains(nameToSearch))
                        .OrderBy(tz => tz.DisplayName.Length)
                        .FirstOrDefault();
                }
                if (tziFound == null)
                {
                    throw new ArgumentException($"Could not find IANA timezone with name {name}", nameof(name));
                }
            }
#else
            var bestMatch = TZConvert.KnownIanaTimeZoneNames
                .Where(tzName => tzName.ToUpper().Contains(nameToSearch))
                .OrderBy(tzName => tzName.Length)
                .FirstOrDefault();
            if (bestMatch == null)
            {
                throw new ArgumentException($"Could not find IANA timezone with name {name}", nameof(name));
            }
            if (!TZConvert.TryGetTimeZoneInfo(bestMatch, out tziFound))
            {
                throw new ArgumentException($"An exception occurred when trying to convert from IANA to Windows Time Zone for value {tziFound}", nameof(name));
            }
#endif
            return tziFound;
        }

        private static string LocateTimeZoneName(TimeZoneInfo dntzi)
        {
            if (dntzi == null)
            {
                throw new ArgumentException("Value must not be empty", nameof(dntzi));
            }

#if NET6_0_OR_GREATER
            if (!TimeZoneInfo.TryConvertWindowsIdToIanaId(dntzi.Id, out var iana))
            {
                if (!TimeZoneInfo.TryConvertWindowsIdToIanaId(dntzi.Id.Replace("Etc/", ""), out iana))
                {
                    throw new ArgumentException($"Windows time zone id {dntzi.Id} could not be converted to IANA tz id", nameof(dntzi));
                }
            }
#else
            if (!TZConvert.TryWindowsToIana(dntzi.Id, out var iana))
            {
                if (!TZConvert.TryWindowsToIana(dntzi.Id.Replace("Etc/", ""), out iana))
                {
                    throw new ArgumentException($"Windows time zone id {dntzi.Id} could not be converted to IANA tz id", nameof(dntzi));
                }
            }
#endif

            var nameToSearch = iana.ToUpper();
            var bestMatch = HaystackTimeZoneDatabase.TimeZones
                .Where(tzName => nameToSearch.Contains(tzName.ToUpper()))
                .OrderByDescending(tzName => tzName.Length)
                .FirstOrDefault();
            if (bestMatch == null)
            {
                throw new ArgumentException($"Windows time zone id {dntzi.Id} converted to IANA id {iana} could not be found in known Haystack ids", nameof(dntzi));
            }
            return bestMatch;
        }
    }
}