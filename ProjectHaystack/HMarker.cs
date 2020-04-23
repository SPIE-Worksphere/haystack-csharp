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
     * HMarker is the singleton value for a marker tag.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     */

    public class HMarker : HVal
    {
        public static readonly HMarker VAL = new HMarker();

        private HMarker()
        {
            // Nothing to do
        }

        // Hash code 
        public override int GetHashCode() { return 0x1379de; }

        // Equals is based on reference 
        public override bool Equals(object that)
        {
            return that is HMarker && this == (HMarker)that;
        }

        // Encode as "marker" 
        public override string ToString() { return "marker"; }

        // Encode as "m:" 
        public override string toJson() { return "m:"; }

        // Encode as "M" 
        public override string toZinc() { return "M"; }
    }
}