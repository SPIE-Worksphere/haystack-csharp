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
    public class HStr : HVal
    {
        // Singleton Instance for empty instance
        private static HStr m_emptyInst;

        // Constructor
        protected HStr(string val)
        {
            Value = val;
        }
        // Access to Instance empty
        public static HStr InstanceEmpty
        {
            get
            {
                if (m_emptyInst == null)
                {
                    m_emptyInst = new HStr("");
                }
                return m_emptyInst;
            }
        }
        // access to HStr
        public static HStr make(string val)
        {
            if (val == null) return null;
            if (val.Length == 0) return InstanceEmpty;
            return (new HStr(val));
        }
        // Access to internal string value
        public string Value { get; }

        public override int GetHashCode() => Value.GetHashCode();
        // HVal hequals
        public override bool Equals(object that)
        {
            if (!(that is HStr)) return false;
            if (((HStr)that).Value == Value) return true;
            else return false;
        }

        public override string ToString()
        {
            return Value;
        }

        // HVal toJson
        // If it contains a colon retunr s:<value> else just the value
        public override string toJson()
        {
            return Value.IndexOf(':') < 0 ? Value : "s:" + Value;
        }

        // Encode using double quotes and back slash escapes 
        public override string toZinc()
        {
            StringBuilder sb = new StringBuilder();
            toZinc(ref sb, Value);
            return sb.ToString();
        }

        public static string toCode(string val)
        {
            StringBuilder sb = new StringBuilder();
            toZinc(ref sb, val);
            return sb.ToString();
        }

        // Encode using double quotes and back slash escapes 
        public static void toZinc(ref StringBuilder sb, string val)
        {
            sb.Append('"');
            for (int i = 0; i < val.Length; ++i)
            {
                int c = val[i];
                if (c < ' ' || c == '"' || c == '\\')
                {
                    sb.Append('\\');
                    switch (c)
                    {
                        case ('\n'): { sb.Append('n'); } break;
                        case ('\r'): { sb.Append('r'); } break;
                        case ('\t'): { sb.Append('t'); } break;
                        case ('"'): { sb.Append('"'); } break;
                        case ('\\'): { sb.Append('\\'); } break;
                        default:
                            {
                                sb.Append('u').Append('0').Append('0');
                                if (c <= 0xf) sb.Append('0');
                                sb.Append(Convert.ToByte(c).ToString("x2"));
                            }
                            break;
                    }
                }
                else
                {
                    sb.Append((char)c);
                }
            }
            sb.Append('"');
        }

        // I have not implemented custom split - I don't see why it is required except it has a Trim included
        public static string[] customSplitWithTrim(string str, char[] cSeps, bool bEmpty)
        {
            string[] strARet = null;
            if (bEmpty)
                strARet = str.Split(cSeps, StringSplitOptions.RemoveEmptyEntries);
            else
                strARet = str.Split(cSeps);
            for (int i = 0; i < strARet.Length; i++)
                strARet[i] = strARet[i].Trim();
            return strARet;
        }
    }
}