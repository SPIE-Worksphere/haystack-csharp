//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    /**
     * HNum wraps a 64-bit floating point number and optional unit name.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HNum : HVal
    {
        private double m_val;
        private string m_unit;
        // only used within member functions
        private static bool[] unitChars = new bool[128];

        // Private constructor 
        private HNum(double val, string unit)
        {
            LoadUnitChars();
            if (!isUnitName(unit)) throw new ArgumentException("Invalid unit name: " + unit, "unit");
            m_val = val;
            m_unit = unit;
        }
        
        // Member access - readonly
        public double doubleval
        {
            get { return m_val; }
        }
        public string unit
        {
            get { return m_unit; }
        }
        #region MakeFunctions
        // Construct with int and null unit (may have loss of precision) 
        public static HNum make(int val)
        {
            return make(val, null);
        }

        // Construct with int and null/non-null unit (may have loss of precision) 
        public static HNum make(int val, string unit)
        {
            if (val == 0 && unit == null) return ZERO; 
            return new HNum((double)val, unit);
        }

        // Construct with long and null unit (may have loss of precision)
        public static HNum make(long val)
        {
            return make(val, null);
        }

        // Construct with long and null/non-null unit (may have loss of precision) 
        public static HNum make(long val, string unit)
        {
            if (val == 0L && unit == null) return ZERO; 
            return new HNum((double)val, unit);
        }

        // Construct with double and null unit 
        public static HNum make(double val)
        {
            return make(val, null);
        }

        // Construct with double and null/non-null unit 
        public static HNum make(double val, string unit)
        {
            if (val == 0.0 && unit == null) return ZERO; 
            return new HNum(val, unit);
        }
        #endregion // Make Functions
        // Singleton value for zero 
        public static HNum ZERO = new HNum(0.0, null);

        // Singleton value for positive infinity "Inf" 
        public static HNum POS_INF = new HNum(double.PositiveInfinity, null);

        // Singleton value for negative infinity "-Inf" 
        public static HNum NEG_INF = new HNum(double.NegativeInfinity, null);

        // Singleton value for not-a-number "NaN" 
        public static HNum NaN = new HNum(double.NaN, null);

        // Hash code is based on val and unit member variables
        public int hashCode()
        {
            
            long lValAsLong = BitConverter.DoubleToInt64Bits(m_val);
            ulong ulValUnsigned = Convert.ToUInt64(lValAsLong);
            int hash = (int)(ulValUnsigned ^ (ulValUnsigned >> 32));
            if (m_unit != null) hash ^= m_unit.GetHashCode();
            return hash;
        }

        // Equals is based on val and unit (NaN == NaN) 
        public  override bool hequals(object that)
        {
            if (!(that is HNum)) return false;
            HNum x = (HNum)that;
            if (double.IsNaN(m_val)) return double.IsNaN(x.doubleval); 
            if (m_val != x.doubleval) return false;
            if (m_unit == null) return x.unit == null;
            if (x.unit == null) return false;
            return m_unit.Equals(x.unit);
        }

        // Return sort order as negative, 0, or positive 
        public int compareTo(object that)
        {
            if (!(that is HNum)) return 1;
            double thatVal = ((HNum)that).doubleval;
            if (m_val < thatVal) return -1;
            if (m_val == thatVal) return 0;
            return 1;
        }

        // Encode as "n:<float> [unit]" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("n:");
            encode(ref s, true);
            return s.ToString();
        }

        // Encode as floating value followed by optional unit string 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            encode(ref s, false);
            return s.ToString();
        }

        private void encode(ref StringBuilder s, bool spaceBeforeUnit)
        {
            if (m_val == double.PositiveInfinity) s.Append("INF");
            else if (m_val == double.NegativeInfinity) s.Append("-INF");
            else if (double.IsNaN(m_val)) s.Append("NaN");
            else
            {
                // don't encode huge set of decimals if over 1.0
                double abs = m_val; if (abs < 0) abs = -abs;
                if (abs > 1.0)
                {
                    // Changed in unit tests .NET requires we respect thread current culture overriding
                    //   here just creates issues in other classes like zinc reader if we were to compare values.
                    // Haystack tokeniser changed to have a variant decimal seperator that respects 
                    //  current thread Culture decimal seperator.  This could cause international boundary issues
                    //  in which case the user of this needs to create a thread respecting the source location and cultre
                    //  context.
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    //nfi.NumberDecimalSeparator = ".";
                    s.Append(m_val.ToString("#0.####"/*, nfi*/));
                }
                else
                    s.Append(m_val);

                if (unit != null)
                {
                    if (spaceBeforeUnit) s.Append(" ");
                    s.Append(m_unit);
                }
            }
        }

        /**
         * Get this number as a duration of milliseconds.
         * Raise InvalidOperationException if the unit is not a duration unit.
         * NOTE: This is kept even though .NET does not really have a millis
         */
        public long millis()
        {
            string u = m_unit;
            if (u == null) u = "null";
            if ((u.Trim() == "ms") || (u.Trim() == "millisecond")) return (long)m_val;
            if ((u.Trim() == "s") || (u.Trim() == "sec")) return (long)(m_val * 1000.0); // NOTE: A case was taken out of the Java here - it represented an unreachable test
            if ((u.Trim() == "min") || (u.Trim() == "minute")) return (long)(m_val * 1000.0 * 60.0);
            if ((u.Trim() == "h") || (u.Trim() == "hr")) return (long)(m_val * 1000.0 * 3600.0); // NOTE: A case was taken out of the Java here - it represented an unreachable test
            throw new InvalidOperationException("Invalid duration unit: " + u);
        }

        /**
         * Return true if the given string is null or contains only valid unit
         * chars.  If the unit name contains invalid chars return false.  This
         * method does *not* check that the unit name is part of the standard
         * unit database.
         * .NET NOTE: Would do this with a compare string but this is probably faster than
         *    a loop and to keep consistent with other toolkits.
         */
        public static bool isUnitName(string strUnit)
        {
            if (strUnit == null) return true;
            if (strUnit.Length == 0) return false;
            for (int i = 0; i < strUnit.Length; ++i)
            {
                int c = strUnit[i];
                if (c < 128 && !unitChars[c]) return false;
            }
            return true;
        }

        private void LoadUnitChars ()
        {
            for (int i = 'a'; i<='z'; ++i) unitChars[i] = true;
            for (int i = 'A'; i<='Z'; ++i) unitChars[i] = true;
            unitChars['_'] = true;
            unitChars['$'] = true;
            unitChars['%'] = true;
            unitChars['/'] = true;
        }
}
}
