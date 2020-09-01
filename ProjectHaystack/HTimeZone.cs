//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//                              Note: This uses Nuget package TimeZoneConverter
//
using System;
using System.Linq;

namespace ProjectHaystack
{
    /**
     * HTimeZone handles the mapping between Haystack timezone
     * names and DNET timezones.
     * 
     * We will keep in a definition file the flat Haystack timezone db 
     * then iterate the .NET framework timezones to match a timezone to a framework
     * instance - depending on how heavy that turns out to be we may cache
     * it or just do it on demand.  YET TO DO - make this decision
     *
     * @see <a href='http://project-haystack.org/doc/TimeZones'>Project Haystack</a>
     */
    public class HTimeZone
    {
        // Haystack timezone name 
        private string m_strName;

        // Map of Name and Tz
        // cache

        // Private constructor 
        private HTimeZone(string name, TimeZoneInfo dntz)
        {
            m_strName = name;
            this.dntz = dntz;
        }

        #region MakeFunctions
        /// <summary>
        /// Make a HtimeZone by Haystack name
        /// </summary>
        /// <param name="name">Haystack Timezone name</param>
        /// <param name="bChecked">Allow null return or check for error and throw exception</param>
        /// <returns></returns>
        public static HTimeZone make(string name, bool bChecked)
        {
            string strNameToSearch = name;
            if (name.ToUpper().Trim() == "REL") // Known non tzi equivalent - but should be GMT
                strNameToSearch = "GMT";

            TimeZoneInfo tziFound = null;
            try
            {
                string strIANATimeZoneID = "";
                bool bFound = false;
                int iLength = TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.ToArray().Length;
                int iCurIndex = 0;
                while ((!bFound) && (iCurIndex < iLength))
                {
                    if (TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.ToArray()[iCurIndex].ToUpper().Contains(strNameToSearch.ToUpper()))
                    {
                        bFound = true;
                        strIANATimeZoneID = TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.ToArray()[iCurIndex];
                    }
                    iCurIndex++;
                }
                if (bFound)
                {
                    if (!TimeZoneConverter.TZConvert.TryGetTimeZoneInfo(strIANATimeZoneID, out tziFound))
                    {
                        if (bChecked)
                        {
                            // Throw a new exception
                            throw new ArgumentException("Name " + name + " not able to convert from IANA to Windows Time Zone, exception occurred", "name");
                        }
                        else
                        {
                            return null;
                        }
                    }
                    HTimeZone tzReturn = new HTimeZone(name, tziFound);
                    return tzReturn;
                }
                else
                {

                    if (bChecked)
                    {
                        throw new ArgumentException("Name " + name + " not found as valid IANA timezone", "name");
                    }
                    else
                    {
                        return (HTimeZone)null;
                    }
                }
            }
            catch (Exception)
            {
                if (bChecked)
                {
                    throw new ArgumentException("Name " + name + " not able to convert from IANA to Windows Time Zone, exception occurred", "name");
                }
                else
                {
                    return (HTimeZone)null;
                }
            }
            /* Unreachable
            if (bChecked)
            {
                throw new ArgumentException("Name " + name + " not able to convert from IANA to Windows Time Zone, exception occurred", "name");
            }
            else
            {
                return (HTimeZone)null;
            }*/
        }

        public static HTimeZone make(TimeZoneInfo dntzi, bool bChecked)
        {
            // Need to do a reverse lookup for the windows timezone id to the haystack timezone id
            if (dntzi == null)
            {
                if (bChecked)
                {
                    throw new ArgumentException("Windows time zone arguement is null", "dntzi");
                }
                else
                {
                    return null;
                }
            }
            string strIANATZID = "";
            try
            {
                strIANATZID = TimeZoneConverter.TZConvert.WindowsToIana(dntzi.Id);
            }
            catch (Exception)
            {
                if (bChecked)
                    throw new ArgumentException("Windows time zone id " + dntzi.Id + " could not be converted to IANA tz id", dntzi.Id);
                else
                    return null;
            }
            string strHaystackTZID = "";
            bool bFound = false;
            int iCurIndex = 0;
            while ((!bFound) && (iCurIndex < CHaystackTZDB.tzdb.Length))
            {
                if (strIANATZID.ToUpper().Contains(CHaystackTZDB.tzdb[iCurIndex].ToUpper()))
                {
                    bFound = true;
                    strHaystackTZID = CHaystackTZDB.tzdb[iCurIndex];
                }
                iCurIndex++;
            }
            if (bFound)
            {
                return new HTimeZone(strHaystackTZID, dntzi);
            }
            else
            {
                if (bChecked)
                    throw new ArgumentException("Windows time zone id " + dntzi.Id + " converted to IANA tz id " + strIANATZID + " could not be found in Haystack tz id", dntzi.Id);
                else
                    return null;
            }
        }
        #endregion // MakeFunctions

        public override string ToString()
        {
            return m_strName;
        }

        public bool hequals(object obj)
        {
            if (!(obj is HTimeZone)) return false;
            // Compare the timezone by name
            HTimeZone objTZ = (HTimeZone)obj;
            string strGMTDNTZ = "Dubai";
            if ((m_strName == "GMT") || (m_strName == "Rel"))
            {
                if ((obj.ToString().CompareTo(m_strName) == 0) || (obj.ToString().CompareTo(strGMTDNTZ) == 0))
                    return true;
                else return false;
            }
            if (obj.ToString().CompareTo(m_strName) == 0)
                return true;
            else
                return false;
        }

        public TimeZoneInfo dntz { get; }

        public static HTimeZone UTC
        {
            get
            {
                HTimeZone htzRet = null;
                try
                {
                    htzRet = make("UTC", true);
                }
                catch (Exception genexcep)
                {
                    // bubble the exception
                    throw new Exception("Exception at UTC tz make", genexcep);
                }
                return htzRet;
            }
        }
        public static HTimeZone REL
        {
            get
            {
                HTimeZone htzRet = null;
                try
                {
                    htzRet = make("Rel", true);
                }
                catch (Exception genexcep)
                {
                    // bubble the exception
                    throw new Exception("Exception at UTC tz make", genexcep);
                }
                return htzRet;
            }
        }
        public static HTimeZone Default
        {
            get
            {
                TimeZoneInfo tzDefault = TimeZoneInfo.Local;

                HTimeZone htzRet = null;
                try
                {
                    htzRet = make(tzDefault, true);
                }
                catch (Exception genexcep)
                {
                    // bubble the exception
                    throw new Exception("Exception at UTC tz make", genexcep);
                }
                return htzRet;
            }
        }
    }
}