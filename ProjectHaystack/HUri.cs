//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Text;

namespace ProjectHaystack
{
    /**
     * HUri models a URI as a string.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HUri : HVal
    {
        private string m_strVal;

        // Private constructor 
        private HUri(string val) { m_strVal = val; }

        // Access
        public string UriVal { get { return m_strVal; } }

        // Construct from string value 
        public static HUri make(string val)
        {
            if (val.Length == 0) return EMPTY;
            return new HUri(val);
        }

        // Singleton value for empty URI 
        private static HUri EMPTY = new HUri("");

        // Hash code is based on string value 
        public override int GetHashCode() => m_strVal.GetHashCode();

        // Equals is based on string value 
        public override bool Equals(object that)
        {
            return that is HUri && m_strVal.CompareTo(((HUri)that).UriVal) == 0;
        }

        // Return value string. 
        public override string ToString()
        {
            return m_strVal;
        }

        // Encode as "u:<val>" 
        public override string toJson()
        {
            return "u:" + m_strVal;
        }

        // Encode using "`" back ticks 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            s.Append('`');
            for (int i = 0; i < m_strVal.Length; ++i)
            {
                int c = m_strVal[i];
                if (c < ' ') throw new ArgumentException("Invalid URI char '" + m_strVal + "', char='" + (char)c + "'", "uir");
                if (c == '`') s.Append('\\');
                s.Append((char)c);
            }
            s.Append('`');
            return s.ToString();
        }
    }
}