//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    /**
     * XStr is an extended string which is a type name and value
     * encoded as a string. It is used as a generic value when an
     * XStr is decoded without any predefined type.
     */
    public class HXStr : HVal
    {
        // Type name 
        private string m_strType;

        // String value 
        private string m_strVal;

        // Access
        public string Type { get { return m_strType; } }
        public string Val { get { return m_strVal; } }

        public static HVal decode(string type, string val)
        {
            if ("Bin".CompareTo(type) == 0) return HBin.make(val);
            return new HXStr(type, val);
        }

        public static HXStr encode(object val)
        {
            // NOTES: not sure if this will work as expected - might be good to test once
            //      it's use is better understood
            return new HXStr(nameof(val), val.ToString());
        }

        private HXStr(string type, string val)
        {
            if (!isValidType(type)) throw new ArgumentException("Invalid type name: " + type, "type");
            m_strType = type;
            m_strVal = val;
        }

        private static bool isValidType(string t)
        {
            if (string.IsNullOrEmpty(t) || char.IsUpper(t[0])) return false;
            char[] chars = t.ToCharArray();
            for (int i = 0; i < chars.Length; ++i)
            {
                if (char.IsLetter(chars[i])) continue;
                if (char.IsDigit(chars[i])) continue;
                if (chars[i] == '_') continue;
                return false;
            }
            return true;
        }


        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            s.Append(m_strType).Append("(\"").Append(m_strVal).Append("\")");
            return s.ToString();
        }

        public override string toJson()
        {
            // YET TO TO - Implement this
            throw new NotImplementedException();
        }

        public override int GetHashCode() => m_strType.GetHashCode() ^ m_strVal.GetHashCode();

        public override bool Equals(object o)
        {
            // Reference check
            if (this == o) return true;
            // Null or Class type check
            if (o == null || !(o is HXStr)) return false;

            HXStr hxStr = (HXStr)o;

            if (m_strType.CompareTo(hxStr.Type) != 0) return false;
            return (m_strVal.CompareTo(hxStr.Val) == 0);

        }

        public int hashCode()
        {
            int result = m_strType.GetHashCode();
            result = 31 * result + m_strVal.GetHashCode();
            return result;
        }
    }
}
