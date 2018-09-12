//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace ProjectHaystack
{
    /**
     * HTime models a time of day tag value.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HTime : HVal 
    {
        // Hour of day as 0-23 
        private int m_iHour;

        // Minute of hour as 0-59 
        private int m_iMin;

        // Second of minute as 0-59 
        private int m_iSec;

        // Fractional seconds in milliseconds 0-999 
        private int m_ims;

        // Private constructor 
        private HTime(int hour, int min, int sec, int ms)
        {
            m_iHour = hour;
            m_iMin = min;
            m_iSec = sec;
            m_ims = ms;
        }

        // Properties
        public int Hour { get { return m_iHour; } }
        public int Minute { get { return m_iMin; } }
        public int Second { get { return m_iSec; } }
        public int Millisecond { get { return m_ims; } }


        // Make methods
        // Construct with all fields 
        public static HTime make(int hour, int min, int sec, int ms)
        {
            if (hour < 0 || hour > 23) throw new ArgumentException("Invalid hour", "hour");
            if (min < 0 || min > 59) throw new ArgumentException("Invalid min", "min");
            if (sec < 0 || sec > 59) throw new ArgumentException("Invalid sec", "sec");
            if (ms < 0 || ms > 999) throw new ArgumentException("Invalid ms", "ms");
            return new HTime(hour, min, sec, ms);
        }

        // Convenience constructing with ms = 0 
        public static HTime make(int hour, int min, int sec)
        {
            return make(hour, min, sec, 0);
        }

        // Convenience constructing with sec = 0 and ms = 0 
        public static HTime make(int hour, int min)
        {
            return make(hour, min, 0, 0);
        }

        // Initialize from .NET DateTime instance 
        public static HTime make(DateTime dt)
        {
            return new HTime(dt.Hour,
                             dt.Minute,
                             dt.Second,
                             dt.Millisecond);
        }

        // Parse from string fomat "hh:mm:ss.FF" or "hh:mm:ss" or raise FormatException (replaces ParseException)
        public static HTime make(string s)
        {
            string strToConv = s;
            DateTime dtParsed = DateTime.Now;
            string strFormat = "";
            if (s.Contains("."))
            {
                strFormat = "HH:mm:ss.fff";
                // Unit tests show that the fff can't be more than 3 chars
                int iDotPos = strToConv.IndexOf(".");
                if ((strToConv.Length - iDotPos > 3) && (strToConv.Length > 12))
                    strToConv = strToConv.Substring(0, 12);
                else if ((strToConv.Length - iDotPos < 4) && (strToConv.Length < 12))
                {
                    // HH:mm:ss.ff
                    int iAddZeros = 3- (strToConv.Length - iDotPos - 1);
                    for (int i = 0; i < iAddZeros; i++)
                        strToConv += '0';
                }
            }
            else
            {
                strFormat = "HH:mm:ss";
            }
            // Unit tests show that the fff can't be more than 3 chars

            if (!DateTime.TryParseExact(strToConv, strFormat,
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out dtParsed))
            {
                throw new FormatException("Invalid time string: " + s);
            }
            return HTime.make(dtParsed.Hour, dtParsed.Minute, dtParsed.Second, dtParsed.Millisecond);
        }

        // Singleton for midnight 00:00 
        public static readonly HTime MIDNIGHT = new HTime(0, 0, 0, 0);

        // Hash is based on hour, min, sec, ms 
        public int hashCode()
        {
            return (m_iHour << 24) ^ (m_iMin << 20) ^ (m_iSec << 16) ^ m_ims;
        }

        // Equals is based on year, month, day 
        public override bool hequals(object that)
        {
            if (!(that is HTime)) return false;
            HTime x = (HTime)that;
            return ((Hour == x.Hour) && (Minute == x.Minute) && (Second == x.Second) && (Millisecond == x.Millisecond));
        }

        // Return sort order as negative, 0, or positive 
        public override int CompareTo(object obj)
        {
            if (obj == null) return 1;
            HTime x = (HTime)obj;
            if (Hour < x.Hour) return -1; else if (Hour > x.Hour) return 1;
            if (Minute < x.Minute) return -1; else if (Minute > x.Minute) return 1;
            if (Second < x.Second) return -1; else if (Second > x.Second) return 1;
            if (Millisecond < x.Millisecond) return -1; else if (Millisecond > x.Millisecond) return 1;

            return 0;
        }

        // Encode as "h:hh:mm:ss.FFF" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("h:");
            encode(s);
            return s.ToString();
        }

        // Encode as "hh:mm:ss.FFF" 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            encode(s);
            return s.ToString();
        }

        // Package private implementation shared with HDateTime 
        // NOTE: This is not multithreaded safe.  But this is the case for most
        //  of this library code
        private void encode(StringBuilder s)
        {
            if (Hour < 10) s.Append('0');
            s.Append(Hour.ToString());
            s.Append(':');
            if (Minute < 10) s.Append('0');
            s.Append(Minute.ToString());
            s.Append(':');
            if (Second < 10) s.Append('0');
            s.Append(Second);
            if (Millisecond != 0)
            {
                s.Append('.');
                if (Millisecond < 10) s.Append('0');
                if (Millisecond < 100) s.Append('0');
                s.Append(Millisecond);
            }
        }

    }
}
