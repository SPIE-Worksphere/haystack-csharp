//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using ProjectHaystack.io;

namespace ProjectHaystack
{
    /**
     * HDateTimeRange models a starting and ending timestamp
     *
     * @see <a href='http://project-haystack.org/doc/Ops#hisRead'>Project Haystack</a>
     */

    // On project haystack this is not a required type - this toolkit will implement via normal other types unless 
    //   in the development there is found a specific need for such relative 
    public class HDateTimeRange
    {
        // Access
        public HDateTime Start { get; }
        public HDateTime End { get; }

        // Private constructor 
        private HDateTimeRange(HDateTime start, HDateTime end)
        {
            Start = start;
            End = end;
        }

        /**
         * Parse from string using the given timezone as context for
         * date based ranges.  The formats are:
         *  - "today"
         *  - "yesterday"
         *  - "{date}"
         *  - "{date},{date}"
         *  - "{dateTime},{dateTime}"
         *  - "{dateTime}"  // anything after given timestamp
         * Throw ParseException is invalid string format.
         */
        public static HDateTimeRange make(string str, HTimeZone tz)
        {
            // handle keywords
            str = str.Trim();
            if (str.CompareTo("today") == 0) return make(HDate.today(), tz);
            if (str.CompareTo("yesterday") == 0) return make(HDate.today().minusDays(1), tz);

            // parse scalars
            int comma = str.IndexOf(',');
            HVal start = null, end = null;
            if (comma < 0)
            {
                start = new HZincReader(str).readVal();
            }
            else
            {
                start = new HZincReader(str.Substring(0, comma)).readVal();
                end = new HZincReader(str.Substring(comma + 1)).readVal();
            }

            // figure out what we parsed for start,end
            if (start is HDate)
            {
                if (end == null) return make((HDate)start, tz);
                if (end is HDate) return make((HDate)start, (HDate)end, tz);
            }
            else if (start is HDateTime)
            {
                if (end == null) return make((HDateTime)start, HDateTime.now(tz));
                if (end is HDateTime) return make((HDateTime)start, (HDateTime)end);
            }

            throw new FormatException("Invalid HDateTimeRange: " + str);
        }

        // Make for single date within given timezone 
        public static HDateTimeRange make(HDate date, HTimeZone tz)
        {
            return make(date, date, tz);
        }

        // Make for inclusive dates within given timezone 
        public static HDateTimeRange make(HDate start, HDate end, HTimeZone tz)
        {
            return make(start.midnight(tz), end.plusDays(1).midnight(tz));
        }

        // Make from two timestamps 
        public static HDateTimeRange make(HDateTime start, HDateTime end)
        {
            if (!start.TimeZone.hequals(end.TimeZone)) throw new ArgumentException("start.TimeZone != end.TimeZone");
            return new HDateTimeRange(start, end);
        }

        /** Make a range which encompasses the current week.
            The week is defined as Sunday thru Saturday. */
        public static HDateTimeRange thisWeek(HTimeZone tz)
        {
            HDate today = HDate.today();
            HDate sun = today.minusDays(today.weekday() - DayOfWeek.Sunday);
            HDate sat = today.plusDays(DayOfWeek.Saturday - today.weekday());
            return make(sun, sat, tz);
        }

        /** Make a range which encompasses the current month. */
        public static HDateTimeRange thisMonth(HTimeZone tz)
        {
            HDate today = HDate.today();
            HDate first = HDate.make(today.Year, today.Month, 1);
            HDate last = HDate.make(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            return make(first, last, tz);
        }

        /** Make a range which encompasses the current year. */
        public static HDateTimeRange thisYear(HTimeZone tz)
        {
            HDate today = HDate.today();
            HDate first = HDate.make(today.Year, 1, 1);
            HDate last = HDate.make(today.Year, 12, 31);
            return make(first, last, tz);
        }

        /** Make a range which encompasses the previous week.
            The week is defined as Sunday thru Saturday. */
        public static HDateTimeRange lastWeek(HTimeZone tz)
        {
            HDate today = HDate.today();
            HDate prev = today.minusDays(7);
            HDate sun = prev.minusDays(prev.weekday() - DayOfWeek.Sunday);
            HDate sat = prev.plusDays(DayOfWeek.Saturday - prev.weekday());
            return make(sun, sat, tz);
        }

        /** Make a range which encompasses the previous month. */
        public static HDateTimeRange lastMonth(HTimeZone tz)
        {
            HDate today = HDate.today();
            DateTime dttoday = new DateTime(today.Year, today.Month, today.Day);
            DateTime dtLastMonth = dttoday.AddMonths(-1);

            HDate lastMonth = HDate.make(dtLastMonth.Year, dtLastMonth.Month, dtLastMonth.Day);
            HDate first = HDate.make(lastMonth.Year, lastMonth.Month, 1);
            HDate last = HDate.make(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
            return make(first, last, tz);
        }

        /** Make a range which encompasses the previous year. */
        public static HDateTimeRange lastYear(HTimeZone tz)
        {
            HDate today = HDate.today();
            HDate first = HDate.make(today.Year - 1, 1, 1);
            HDate last = HDate.make(today.Year - 1, 12, 31);
            return make(first, last, tz);
        }


        /** Return "start to end" */
        public override string ToString()
        {
            return Start.ToString() + "," + End.ToString();
        }
    }
}