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

namespace ProjectHaystack.io
{
    public class HaystackToken
    {
        // Internal Properties
        private string m_strSymbol;
        public bool m_bliteral;    

        // Access
        public string Symbol { get { return m_strSymbol; } }
        public bool Literal { get { return m_bliteral; } }

        // Static predefined tokens
        public static HaystackToken eof = new HaystackToken("eof");

        public static HaystackToken id = new HaystackToken("identifier");
        public static HaystackToken num = new HaystackToken("Number", true);
        public static HaystackToken str = new HaystackToken("Str", true);
        public static HaystackToken refh = new HaystackToken("Ref", true); // can't use ref in .NET it is a reserved keyword
        public static HaystackToken uri = new HaystackToken("Uri", true);
        public static HaystackToken date = new HaystackToken("Date", true);
        public static HaystackToken time = new HaystackToken("Time", true);
        public static HaystackToken dateTime = new HaystackToken("DateTime", true);

        public static HaystackToken colon = new HaystackToken(":");
        public static HaystackToken comma = new HaystackToken(",");
        public static HaystackToken semicolon = new HaystackToken(";");
        public static HaystackToken minus = new HaystackToken("-");
        public static HaystackToken eq = new HaystackToken("==");
        public static HaystackToken notEq = new HaystackToken("!=");
        public static HaystackToken lt = new HaystackToken("<");
        public static HaystackToken lt2 = new HaystackToken("<<");
        public static HaystackToken ltEq = new HaystackToken("<=");
        public static HaystackToken gt = new HaystackToken(">");
        public static HaystackToken gt2 = new HaystackToken(">>");
        public static HaystackToken gtEq = new HaystackToken(">=");
        public static HaystackToken lbracket = new HaystackToken("[");
        public static HaystackToken rbracket = new HaystackToken("]");
        public static HaystackToken lbrace = new HaystackToken("{");
        public static HaystackToken rbrace = new HaystackToken("}");
        public static HaystackToken lparen = new HaystackToken("(");
        public static HaystackToken rparen = new HaystackToken(")");
        public static HaystackToken arrow = new HaystackToken("->");
        public static HaystackToken slash = new HaystackToken("/");
        public static HaystackToken assign = new HaystackToken("=");
        public static HaystackToken bang = new HaystackToken("!");
        public static HaystackToken nl = new HaystackToken("newline");

        public HaystackToken(string symbol)
        {
            m_strSymbol = symbol;
            m_bliteral = false;
        }

        public HaystackToken(string symbol, bool literal)
        {
            m_strSymbol = symbol;
            m_bliteral = literal;
        }

        public bool hequals(object o)
        {
            if (this == o) return true; // reference check
            if (o == null || (!(o is HaystackToken))) return false; // null and type check

            HaystackToken that = (HaystackToken)o;
            // Value compare
            if (m_bliteral != that.Literal) return false;
            return (m_strSymbol.CompareTo(that.Symbol) == 0);
        }

        public int hashCode()
        {
            int result = m_strSymbol.GetHashCode();
            result = 31 * result + (m_bliteral ? 1 : 0);
            return result;
        }

        public override string ToString()
        {
            return m_strSymbol;
        }
    }

}
