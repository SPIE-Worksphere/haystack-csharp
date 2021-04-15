//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Text;

namespace ProjectHaystack
{
    /**
     * XStr is an extended string which is a type name and value
     * encoded as a string. It is used as a generic value when an
     * XStr is decoded without any predefined type.
     */
    public class HXStr : HVal
    {
        // Access
        public string Type { get; }
        public string Val { get; }

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
            Type = type;
            Val = val;
        }

        private static bool isValidType(string t)
        {
            if (string.IsNullOrEmpty(t) || !char.IsUpper(t[0])) return false;
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
            s.Append(Type).Append("(\"").Append(Val).Append("\")");
            return s.ToString();
        }

        public override string toJson()
        {
            // YET TO TO - Implement this
            throw new NotImplementedException();
        }

        public override int GetHashCode() => Type.GetHashCode() * 31 + Val.GetHashCode();

        public override bool Equals(object o)
        {
            // Reference check
            if (this == o) return true;
            // Null or Class type check
            if (o == null || !(o is HXStr)) return false;

            HXStr hxStr = (HXStr)o;

            if (Type.CompareTo(hxStr.Type) != 0) return false;
            return (Val.CompareTo(hxStr.Val) == 0);
        }
    }
}