//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//                           Uses Dot Net DateTime for most operations
//
using System;
using System.Globalization;
using System.Text;

namespace ProjectHaystack
{
    /**
     * HDate models a date (day in year) tag value.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    /* Porting note: A lot of functionality replaced with C# DateTime */
    public class HDate : HVal
    {
        // Properties for access (replicates public final in java implementation
        public int Year { get; }
        public int Month { get; }
        public int Day { get; }

        // Private constructor 
        private HDate(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }
        // Make implementations
        // Construct from basic fields 
        public static HDate make(int year, int month, int day)
        {
            if (year < 1900) throw new ArgumentException("Invalid year", "year");
            if (month < 1 || month > 12) throw new ArgumentException("Invalid Month", "month");
            if (day < 1 || day > 31) throw new ArgumentException("Invalid Day", "day");
            try
            {
                DateTime dtCheck = new DateTime(year, month, day);
            }
            catch (Exception)
            {
                throw new ArgumentException("Combination of year, month, day represent invalid date");
            }
            return new HDate(year, month, day);
        }

        // Construct from csharp datetime instance 
        public static HDate make(DateTime dt)
        {
            return new HDate(dt.Year,
                             dt.Month,
                             dt.Day);
        }

        // Parse from string fomat "YYYY-MM-DD" or raise FormatException (ParseException)
        public static HDate make(string s)
        {
            DateTime dtParsed = DateTime.Now;
            if (!DateTime.TryParseExact(s, "yyyy'-'MM'-'dd",
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dtParsed))
            {
                throw new FormatException("Invalid date string: " + s);
            }
            return HDate.make(dtParsed.Year, dtParsed.Month, dtParsed.Day);
        }

        // Get HDate for current time in default timezone 
        public static HDate today()
        {
            return HDateTime.now().date;
        }

        public override int GetHashCode() => ToDateTime().GetHashCode();
        public override bool Equals(object obj)
        {
            return obj is HDate && (Year == ((HDate)obj).Year && Month == ((HDate)obj).Month && Day == ((HDate)obj).Day);
        }

        // Encode as "d:YYYY-MM-DD" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("d:");
            encode(ref s);
            return s.ToString();
        }
        // Encode as "YYYY-MM-DD" 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            encode(ref s);
            return s.ToString();
        }
        // Package private implementation shared with HDateTime 
        protected void encode(ref StringBuilder s)
        {
            s.Append(Year).Append('-');
            if (Month < 10) s.Append('0');
            s.Append(Month).Append('-');
            if (Day < 10) s.Append('0');
            s.Append(Day);
        }

        // Convert this date into HDateTime for midnight in given timezone. 
        public HDateTime midnight(HTimeZone tz)
        {
            return HDateTime.make(this, HTime.MIDNIGHT, tz);
        }

        public HDate plusDays(int numDays)
        {
            if (numDays == 0) return this;
            if (numDays < 0) return minusDays(numDays * -1);
            DateTime dtNow = new DateTime(Year, Month, Day);
            DateTime dtPlus = dtNow.AddDays(numDays);
            return make(dtPlus.Year, dtPlus.Month, dtPlus.Day);
        }

        /** Return date in past given number of days */
        public HDate minusDays(int numDays)
        {
            if (numDays == 0) return this;
            if (numDays < 0) return plusDays(numDays * -1);
            DateTime dtNow = new DateTime(Year, Month, Day);
            DateTime dtPlus = dtNow.AddDays(numDays * -1);
            return make(dtPlus.Year, dtPlus.Month, dtPlus.Day);
        }

        /** Return if given year a leap year */
        public static bool isLeapYear(int year)
        {
            if ((year & 3) != 0) return false;
            return (year % 100 != 0) || (year % 400 == 0);
        }

        public DayOfWeek weekday()
        {
            DateTime now = new DateTime(Year, Month, Day);
            return (now.DayOfWeek);
        }

        public DateTime ToDateTime() => new DateTime(Year, Month, Day);
    }
}