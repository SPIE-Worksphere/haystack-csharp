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
     * HBin models a binary file with a MIME type.
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tagKinds'>Project Haystack</a>
     * 
     * See verifyMime for some implementation concerns from the direct Java porting
     */
    public class HBin : HVal
    {
        public string mime { get; }

        // Construct for MIME type 
        public static HBin make(string strMime)
        {
            if ((strMime == null) || (strMime.Length) == 0 || (strMime.IndexOf('/') < 0))
                throw new ArgumentException("Invalid mime val: \"" + strMime + "\"", "strMime");
            return new HBin(strMime);
        }

        // Private constructor 
        private HBin(string mime)
        {
            verifyMime(mime);
            this.mime = mime;
        }


        // Hash code is based on mime field 
        public override int GetHashCode() => mime.GetHashCode();

        // Equals is based on mime field 
        public override bool Equals(object that)
        {
            return that is HBin && mime.Equals(((HBin)that).mime);
        }

        // Encode as "b:<mime>" 
        public override string toJson()
        {
            StringBuilder s = new StringBuilder();
            s.Append("b:");
            s.Append(mime);
            return s.ToString();
        }

        // Encode as Bin("<mime>") 
        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            s.Append("Bin(").Append('"').Append(mime).Append('"').Append(')');
            return s.ToString();
        }

        private static void verifyMime(string strMime)
        {
            // don't agree with this code - haystack.org has HBin as binary blob with a MIME type formatted as Bin(tet/plain)
            // Mime here is checking for some plain text but the binary blob is missing - don't know how to transmit that over RESTApi/JSON
            // Also don't understand why end bracket is excluded as the only character from plain text
            // Also don't understand why unprintable non plain text characters like NULL etc are included in this implementation.
            // Need to understand this.  
            // YET TO DO - for the moment leaving this as a direct port from the Java toolkit but need to see this tested and work as per specification
            for (int i = 0; i < strMime.Length; ++i)
            {
                int c = strMime[i];
                if (c > 127 || c == ')') throw new ArgumentException("Invalid mime, char='" + (char)c + "'", "strMime");
            }
        }
    }
}