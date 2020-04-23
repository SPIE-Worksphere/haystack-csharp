//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Globalization;
using System.Text;
using ProjectHaystack.io;

namespace ProjectHaystack
{
    public class HCoord : HVal
    {
        // Private Constructor
        private HCoord(int ulat, int ulng)
        {
            if (ulat < -90000000 || ulat > 90000000) throw new ArgumentException("Invalid lat > +/- 90", "ulat");
            if (ulng < -180000000 || ulng > 180000000) throw new ArgumentException("Invalid lng > +/- 180", "ulng");
            this.ulat = ulat;
            this.ulng = ulng;
        }

        // Parse from string fomat "C(lat,lng)" or raise ParseException 
        public static HCoord make(string s)
        {
            return (HCoord)new HZincReader(s).readVal();
        }

        // Static Make method
        public static HCoord make(double dblLat, double dblLng)
        {
            return new HCoord((int)(dblLat * 1000000.0), (int)(dblLng * 1000000.0));
        }

        // Return if given latitude is legal value between -90.0 and +90.0 
        public static bool isLat(double lat)
        {
            return ((-90.0 <= lat) && (lat <= 90.0));
        }

        // Return if given is longtitude is legal value between -180.0 and +180.0 
        public static bool isLng(double lng)
        {
            return ((-180.0 <= lng) && (lng <= 180.0));
        }

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        // Latitude in decimal degrees 
        public double lat
        {
            get
            {
                return (ulat / 1000000.0);
            }
        }

        // Longtitude in decimal degrees 
        public double lng
        {
            get
            {
                return (ulng / 1000000.0);
            }
        }

        public int ulat { get; }
        public int ulng { get; }

        // Hash is based on lat/lng 
        public override int GetHashCode() { return (ulat << 7) ^ ulng; }

        // Equality is based on lat/lng 
        public override bool hequals(object that)
        {
            if (!(that is HCoord)) return false;
            HCoord x = (HCoord)that;
            return ((ulat == x.ulat) && (ulng == x.ulng));
        }

        // Return "c:lat,lng" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("c:");
            s.Append(uToStr(ulat));
            s.Append(',');
            s.Append(uToStr(ulng));
            return s.ToString();
        }

        // Represented as "C(lat,lng)" 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            s.Append("C(");
            s.Append(uToStr(ulat));
            s.Append(',');
            s.Append(uToStr(ulng));
            s.Append(')');
            return s.ToString();
        }

        private static string uToStr(int ud)
        {
            StringBuilder s = new StringBuilder();
            if (ud < 0) { s.Append('-'); ud = -ud; }
            if (ud < 1000000.0)
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                double decValue = (double)(ud / 1000000.0);
                // Format string with locale with a format of 0.045 note 0's mean it must be there e.g. 1 decimal place resolution
                //s.Append(new DecimalFormat("0.0#####", new DecimalFormatSymbols(Locale.ENGLISH)).format(ud / 1000000.0));
                s.Append(decValue.ToString("0.0#####", culture));
                return s.ToString();
            }
            string x = ud.ToString();
            int dot = x.Length - 6;
            int end = x.Length;
            while (end > dot + 1 && x[end - 1] == '0') --end;
            for (int i = 0; i < dot; ++i) s.Append(x[i]);
            s.Append('.');
            for (int i = dot; i < end; ++i) s.Append(x[i]);
            return s.ToString();
        }
    }
}