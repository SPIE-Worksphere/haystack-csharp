//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//

namespace ProjectHaystack
{
    /**
     * HBool defines singletons for true/false tag values.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */
    public class HBool : HVal
    {
        // Singleton value for true 
        public static readonly HBool TRUE = new HBool(true);

        // Singleton value for false 
        public static readonly HBool FALSE = new HBool(false);

        private HBool(bool val)
        {
            this.val = val;
        }

        public static HBool make(bool bVal)
        {
            return (new HBool(bVal));
        }
        // Hash code is same as java.lang.Boolean 
        public override int GetHashCode() => val.GetHashCode();

        // Equals is based on reference 
        public override bool Equals(object that)
        {
            return that is HBool && val == ((HBool)that).val;
        }

        public bool val { get; }
        // Encode as "true" or "false" 
        public override string ToString()
        {
            return val ? "true" : "false";
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
            return val ? "T" : "F";
        }
    }
}