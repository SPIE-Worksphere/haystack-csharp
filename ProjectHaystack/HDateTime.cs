//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ProjectHaystack
{
    /**
     * HDateTime models a timestamp with a specific timezone.
     *
     *         DateTime: an ISO 8601 timestamp followed by timezone name:
     *           2011-06-07T09:51:27-04:00 New_York
     *           2012-09-29T14:56:18.277Z UTC
     *         NOTE: In this case we have assumed the offset is derived from parse - this does not offer an indendant
     *           make with an offset outside of the ISO string parse - too much room for error.
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HDateTime : HVal
    {
        // Offset in seconds from UTC including DST offset 
        // We will store this in a dn DateTimeOffset as this is what we parse with
        private DateTimeOffset m_dtoParsed;

        // Member Access 
        public HDate date { get; }

        public HTime time { get; }

        public TimeSpan Offset { get { return m_dtoParsed.Offset; } }

        public HTimeZone TimeZone { get; }

        public long Ticks { get { return m_dtoParsed.Ticks; } }

        public DateTimeOffset CopyOfDTO
        {
            get
            {
                // Get a copy of the DTO
                DateTimeOffset dtoRet = new DateTimeOffset(m_dtoParsed.DateTime, m_dtoParsed.Offset);
                return dtoRet;
            }
        }

        // Private constructor 
        private HDateTime(HDate date, HTime time, HTimeZone tz)
        {
            this.date = date;
            this.time = time;
            TimeZone = tz;
            m_dtoParsed = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Millisecond, tz.dntz.BaseUtcOffset);
            // NOTE: Here offset is fixed - normal path is through other constructor for a parsed ISO string
        }
        private HDateTime(DateTimeOffset dto, HTimeZone htz)
        {
            m_dtoParsed = dto;
            TimeZone = htz;
            date = HDate.make(dto.Year, dto.Month, dto.Day);
            time = HTime.make(dto.Hour, dto.Minute, dto.Second, dto.Millisecond);
        }
        #region MakeFunctions

        // Constructor with basic fields 
        public static HDateTime make(HDate date, HTime time, HTimeZone tz)
        {
            if (date == null || time == null || tz == null) throw new ArgumentException("null args");
            return new HDateTime(date, time, tz);
        }
        // Constructor with date and time (to sec) fields 
        public static HDateTime make(int year, int month, int day, int hour, int min, int sec, HTimeZone tz)
        {
            return make(HDate.make(year, month, day), HTime.make(hour, min, sec), tz);
        }

        // Constructor with date and time (to min) fields 
        public static HDateTime make(int year, int month, int day, int hour, int min, HTimeZone tz)
        {
            return make(HDate.make(year, month, day), HTime.make(hour, min), tz);
        }

        // Constructor with ticks since DN epoch and local timezone 
        public static HDateTime make(long ticks)
        {
            return make(ticks, HTimeZone.Default);
        }

        // Constructor with ticks and DN TimeZone instance 
        public static HDateTime make(long ticks, HTimeZone tz)
        {
            // use DateTimeOffset to decode ticks to fields
            DateTimeOffset dt = new DateTimeOffset(ticks, tz.dntz.BaseUtcOffset);

            HDateTime ts = new HDateTime(dt, tz);
            return ts;
        }

        /* Parse from string ISO8601 excluding W specifier:
         * fomats accepted are - "yyyy-MM-dd'T'HH:mm:ss.FFFtzooffsetname"   - Year Month Date Hour minute second and non zero miliseconds then the 
         *                                                                  Zone offset with name
         *                     - "yyyy-MM-dd'T'HH:mm:ss.FFFZ UTC"           - Year Month Date Hour minute second and non zero miliseconds then the 
         *                                                                  Zone offset as Z specifier                                   
         *                     - "yyyy-MM-dd'T'HH:mm:sstzoffsetname"        - Same but without millisecond - assume millisecond is zero
         *                     - "yyyy-MM-dd'T'HH:mm:ssZ UTC"               - Same but without millisecond - assume millisecond is zero (offset as Z specifier)
         *                     - "yyyy-MM-dd'T'HH:mm:ss"                    - Same but without millisecond and offset 
         * or raise ParseException
         */
        /* Haven't dealt with this format - need to find correct method to meet Haystack specificaiton here */
        public static HDateTime make(string s, bool bException)
        {
            // Tested 17.06.2018 with 2018-06-17T13:00:00.123+10:00 Melbourne
            string sCurrent = s;
            bool bUTC = false;
            bool bNoZoneOffset = false;
            string strDateTimeOnly = "";
            string strDateTimeOffsetFormatSpecifier = "";
            string strHTimeZone = "";
            int iPosOfLastSecondChar, iPosOfSpaceBeforeTZID = 0;

            DateTimeOffset dto;
            TimeSpan tsOffset = new TimeSpan();
            if (sCurrent.Contains('W'))
            {
                if (bException)
                    throw new FormatException("Invalid DateTime format for string " + s + " ISO8601 W specifier not supported by this toolkit");
                else
                    return null;
            }
            if (sCurrent.Trim().Contains("Z UTC"))
            {
                bUTC = true;
                int iPos = sCurrent.IndexOf('Z');
                iPosOfLastSecondChar = iPos - 1;
                strDateTimeOnly = sCurrent.Substring(0, iPos);
            }
            else if (sCurrent.Trim().Contains("Z"))
            {
                bUTC = true;
                int iPos = sCurrent.IndexOf('Z');
                iPosOfLastSecondChar = iPos - 1;
                strDateTimeOnly = sCurrent.Substring(0, iPos);
            }
            if (!sCurrent.Contains('T'))
            {
                // Only possible in ISO 8601 with just a date - this is not an allowed case for HDateTime
                if (bException)
                    throw new FormatException("Invalid DateTime format for string " + s + " missing ISO8601 T specifier");
                else
                    return null;
            }
            // if it is offset with name then it must have a + or - sign for the offset after the T 
            int iPosOfT = sCurrent.IndexOf('T');
            if (iPosOfT + 1 >= sCurrent.Length)
            {
                // Nothing after 'T' this is not legall
                if (bException)
                    throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier");
                else
                    return null;
            }

            // Stip of the timezone by finding the space
            iPosOfSpaceBeforeTZID = sCurrent.IndexOf(' ', iPosOfT);
            int iEndLen = iPosOfSpaceBeforeTZID - (iPosOfT + 1);
            string sEnd = sCurrent.Substring(iPosOfT + 1, iEndLen);
            if (!bUTC)
            {
                if ((sEnd.Trim().Contains('+')) || (sEnd.Trim().Contains('-')))
                {
                    bool bPositive = false;
                    int iPosSign = 0;
                    // In ISO 8601 this is either a +/-hh:mm or +/-hhmm or +/-hh
                    // See how many characters there is till space - that is the offset specifier
                    if (sEnd.Trim().Contains('+'))
                    {
                        bPositive = true;
                        iPosSign = sCurrent.IndexOf('+', iPosOfT);
                    }
                    else
                        iPosSign = sCurrent.IndexOf('-', iPosOfT);
                    iPosOfLastSecondChar = iPosSign - 1;
                    strDateTimeOnly = sCurrent.Substring(0, iPosSign);
                    // Find the next space - requires it contains a Haystack Zone specifier if not UTC
                    int iPosSpace = sCurrent.Trim().IndexOf(' ', iPosSign);
                    if ((iPosSpace < iPosSign) || (iPosSpace < 0))
                    {
                        if (bException)
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier");
                        else
                            return null;
                    }
                    // What is the number of characters between the sign and the space - 5 4 or 2
                    iPosOfSpaceBeforeTZID = iPosSpace;
                    int iNumOffsetChars = iPosSpace - iPosSign - 1;
                    string strOffset = sCurrent.Substring(iPosSign + 1, iNumOffsetChars);
                    if (iNumOffsetChars == 5)
                    {
                        // Assume +/-hh:mm
                        string[] strHM = strOffset.Split(':');
                        if (strHM.Length != 2)
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }

                        int iHour;
                        if (!int.TryParse(strHM[0], out iHour))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if (iHour < 0)
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }

                        int iMinute;
                        if (!int.TryParse(strHM[1], out iMinute))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if ((iMinute < 0) || (iMinute > 60))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if (!bPositive)
                        {
                            // Problem: if minute is non-zero we will get undesired result e.g. -10:10 will return -9:50 therefore both must be negative
                            iMinute = iMinute * -1;
                            iHour = iHour * -1;
                        }
                        tsOffset = new TimeSpan(iHour, iMinute, 0);
                    }
                    else if (iNumOffsetChars == 4)
                    {
                        int iHour;
                        // Assume hhmm
                        if (!Int32.TryParse(strOffset.Substring(0, 2), out iHour))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if (iHour < 0)
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }

                        int iMinute;
                        if (!int.TryParse(strOffset.Substring(2, 2), out iMinute))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if ((iMinute < 0) || (iMinute > 60))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if (!bPositive)
                        {
                            // Problem: if minute is non-zero we will get undesired result e.g. -10:10 will return -9:50 therefore both must be negative
                            iMinute = iMinute * -1;
                            iHour = iHour * -1;
                        }
                        tsOffset = new TimeSpan(iHour, iMinute, 0);
                    }
                    else if (iNumOffsetChars == 2)
                    {
                        // Assume hh
                        int iHour;
                        // Assume hhmm
                        if (!int.TryParse(strOffset, out iHour))
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if (iHour < 0)
                        {
                            // Invalid offset
                            if (bException)
                                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                            else
                                return null;
                        }
                        if (!bPositive)
                        {
                            iHour = iHour * -1;
                        }
                        tsOffset = new TimeSpan(iHour, 0, 0);
                    }
                    else
                    {
                        // Invalid offset
                        if (bException)
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        else
                            return null;
                    }

                }
                else
                {
                    // Must not contain a Zone offset string
                    bNoZoneOffset = true;
                    iPosOfSpaceBeforeTZID = sCurrent.IndexOf(' ', iPosOfT);
                    strDateTimeOnly = sCurrent.Substring(0, iPosOfSpaceBeforeTZID);
                }
            }
            // Get the Haystack time zone identifier and create the HTimeZone object.
            if ((iPosOfSpaceBeforeTZID < sCurrent.Length) && (sCurrent.Length - iPosOfSpaceBeforeTZID > 2) && (!bUTC))
                strHTimeZone = sCurrent.Substring(iPosOfSpaceBeforeTZID + 1);
            // Check for the 'T' for the Formats that include that
            // Check for milliseconds
            if (strDateTimeOnly.Trim().Contains("."))
            {
                // All fields with 'T'
                strDateTimeOffsetFormatSpecifier = "yyyy-MM-dd'T'HH:mm:ss.FFF";
            }
            else
            {
                // no milliseconds
                // DateTimeOffset will adopt 0 milliseconds
                strDateTimeOffsetFormatSpecifier = "yyyy-MM-dd'T'HH:mm:ss";
            }
            try
            {
                DateTime dt = DateTime.ParseExact(strDateTimeOnly, strDateTimeOffsetFormatSpecifier,
                                                                    CultureInfo.InvariantCulture);
                if (!bNoZoneOffset)
                    dto = new DateTimeOffset(dt, tsOffset);
                else if (bUTC)
                {
                    DateTime dtutc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    dto = new DateTimeOffset(dtutc);
                }
                else
                    dto = new DateTimeOffset(dt);
            }
            catch (Exception)
            {
                if (bException)
                    throw new FormatException("Invalid DateTime format for string " + s);
                else return null;
            }
            HTimeZone htz;
            if (bUTC)
            {
                htz = HTimeZone.UTC;
            }
            else
            {
                try
                {
                    htz = HTimeZone.make(strHTimeZone, true);
                }
                catch (Exception genexcep)
                {
                    if (bException)
                        throw new FormatException("Invalid DateTime format for string " + s + " for Timezone [" + genexcep.Message + "]");
                    else return null;
                }
            }
            return new HDateTime(dto, htz);
        }

        // Get HDateTime for given timezone 
        public static HDateTime now(HTimeZone tz)
        {
            return make(DateTime.Now.Ticks, tz);
        }

        public static HDateTime now()
        {
            return make(DateTime.Now.Ticks);
        }

        #endregion // MakeFunctions

        // Encode as "t:YYYY-MM-DD'T'hh:mm:ss.FFFz zzzz" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("t:");
            s.Append(ToString());
            return s.ToString();
        }

        // Encode as "YYYY-MM-DD'T'hh:mm:ss.FFFz zzzz" 
        public override string toZinc()
        {
            return ToString();
        }

        public override int GetHashCode() => date.GetHashCode() ^ time.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is HDateTime
                && date.hequals(((HDateTime)obj).date)
                && time.hequals(((HDateTime)obj).time)
                && m_dtoParsed.Offset == ((HDateTime)obj).Offset
                && TimeZone.hequals(((HDateTime)obj).TimeZone);
        }

        public override string ToString()
        {
            var strRet = m_dtoParsed.ToString("yyyy-MM-dd'T'HH:mm:ss.FFF");
            if (TimeZone.ToString() == "UTC")
            {
                strRet += "Z UTC";
                return strRet;
            }
            else if (m_dtoParsed.Offset.Hours > 0) // Add the "+" sign if positive
                strRet += "+";
            else
                strRet += "-";
            strRet += m_dtoParsed.Offset.ToString(@"hh\:mm");
            strRet += " ";
            strRet += TimeZone.ToString();
            return strRet;
        }

        public int CompareTo(HDateTime that)
        {
            // Original Java compared millis which is the milliseconds sicne epoch at UTC
            //   equivalent for C# = compare the datetime offset for UTC 
            DateTimeOffset dtoCopy = that.CopyOfDTO;
            return (m_dtoParsed.ToUniversalTime().CompareTo(dtoCopy.ToUniversalTime()));
        }
    }
}