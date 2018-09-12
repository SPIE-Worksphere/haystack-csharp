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
     * HBool defines singletons for true/false tag values.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HBool : HVal
    {
        private bool m_value;

        // Singleton value for true 
        public static readonly HBool TRUE = new HBool(true);

        // Singleton value for false 
        public static readonly HBool FALSE = new HBool(false);

        private HBool(bool val)
        {
            m_value = val;
        }
        
        public static HBool make (bool bVal)
        {
            return (new HBool(bVal));
        }
        // Hash code is same as java.lang.Boolean 
        public int hashCode()
        {
            return m_value.GetHashCode();
        }

        // Equals is based on reference 
        public override bool hequals(object that)
        {
            if (that is HBool)
                return (m_value == ((HBool)that).val);
            else return false;
        }

        public bool val
        {
            get { return m_value; }
        }
        // Encode as "true" or "false" 
        public override string ToString()
        {
            return m_value ? "true" : "false";
        }

        public override string toJson()
        {
            // I am not sure this is 100% really it should be { "<field name>": <true/false> } - but we don't know the field
            //  I assume this is called in building up the value in this above not needed for the whole thing
            return ToString();
        }

        /** Encode as "T" or "F" */
        public override string toZinc()
        {
            return m_value ? "T" : "F";
        }
    }
}
