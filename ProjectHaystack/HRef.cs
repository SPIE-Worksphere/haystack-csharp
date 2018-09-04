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

namespace ProjectHaystack
{
    /**
     * HRef wraps a string reference identifier and optional display name.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HRef : HVal
    {
        // String identifier for reference 
        private string m_val;

        // Display name for reference or null 
        private string m_dis;

        
        #region memberAccess
        public string val
        {
            get { return m_val; }
        }
        public bool disSet
        {
            get { return (m_dis != null); }
        }
        #endregion
        /* commented out as it creates confusion over display and Tostring methods
        public string dis
        {
            get { return m_dis; }
        }
        
        */
        #region makeFunctions
        // Construct for string identifier and optional display
        public static HRef make(string val, string dis)
        {
            if (val == null || !isId(val)) throw new ArgumentException("Invalid id val: \"" + val + "\"");
            return new HRef(val, dis);
        }

        // Construct for string identifier and null display 
        public static HRef make(string val)
        {
            return make(val, null);
        }

        #endregion // makeFunctions
        
        // Private constructor 
        private HRef(string val, string dis)
        {
            m_val = val;
            m_dis = dis;
        }

        // Hash code is based on val field only 
        public int hashCode()
        {
            return m_val.GetHashCode();
        }

        // Equals is based on val field only 
        public override bool hequals(object that)
        {
            if (!(that is HRef)) return false;
            return (m_val.CompareTo(((HRef)that).ToString()) == 0);
        }

        // Return display string which is dis field if non-null, val field otherwise 
        public string display()
        {
            if (m_dis != null) return m_dis;
            return m_val;
        }

        // Return the val string 
        public override string ToString()
        {
            return m_val;
        }

        // Encode as "@id" 
        public string toCode()
        {
            return ("@" + m_val);
        }

        // Encode as "r:<id> [dis]" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("r:").Append(m_val);
            if (m_dis != null) s.Append(' ').Append(m_dis);
            return s.ToString();
        }

        // Encode as "@<id> [dis]" 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            s.Append('@');
            s.Append(m_val);
            if (m_dis != null)
            {
                s.Append(' ');
                HStr.toZinc(ref s, m_dis); 
            }
            return s.ToString();
        }

        // Return if the given string is a valid id for a reference 
        public static bool isId(string id)
        {
            if (id.Length == 0) return false;
            for (int i = 0; i < id.Length; ++i)
                if (!isIdChar(id[i])) return false;
            return true;
        }

        // Is the given character valid in the identifier part 
        public static bool isIdChar(int ch)
        {
            // Tested for .NET 24.06.2018
            return ch >= 0 && ch < idChars.Length && idChars[ch];
        }

        // Singleton for the null ref 
        public static HRef nullRef = new HRef("null",  null);

        private static bool[] idChars = loadIDChars();
        private static bool[] loadIDChars ()
        {
            bool[] bARet = new bool[127];
            for (int i = 'a'; i<='z'; ++i) bARet[i] = true;
            for (int i = 'A'; i<='Z'; ++i) bARet[i] = true;
            for (int i = '0'; i<='9'; ++i) bARet[i] = true;
            bARet['_'] = true;
            bARet[':'] = true;
            bARet['-'] = true;
            bARet['.'] = true;
            bARet['~'] = true;
            return bARet;
        }
    }
}
