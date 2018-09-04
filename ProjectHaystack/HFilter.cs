//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectHaystack.io;

namespace ProjectHaystack
{
    /**
     * HFilter models a parsed tag query string.
     *
     * @see <a href='http://project-haystack.org/doc/Filters'>Project Haystack</a>
     */
    public abstract class HFilter
    {
        private string m_filString;

        //////////////////////////////////////////////////////////////////////////
        // Encoding
        //////////////////////////////////////////////////////////////////////////

        /** Convenience for "make(s, true)" */
        public static HFilter make(string s) { return make(s, true); }

        /** Decode a string into a HFilter; return null or throw
            ParseException if not formatted correctly */
        public static HFilter make(string s, bool bChecked)
        {
            try
            {
                return new FilterParser(s).parse();
            }
            catch (Exception e)
            {
                if (!bChecked) return null;
                if (e is FormatException) throw (FormatException)e;
                throw new FormatException(s, e);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Factories
        //////////////////////////////////////////////////////////////////////////

        /**
         * Match records which have the specified tag path defined.
         */
        public static HFilter has(string path) { return new Has(Path.make(path)); }

        /**
         * Match records which do not define the specified tag path.
         */
        public static HFilter missing(string path) { return new Missing(Path.make(path)); }

        /**
         * Match records which have a tag are equal to the specified value.
         * If the path is not defined then it is unmatched.
         */
        public static HFilter eq(string path, HVal val) { return new Eq(Path.make(path), val); }

        /**
         * Match records which have a tag not equal to the specified value.
         * If the path is not defined then it is unmatched.
         */
        public static HFilter ne(string path, HVal val) { return new Ne(Path.make(path), val); }

        /**
         * Match records which have tags less than the specified value.
         * If the path is not defined then it is unmatched.
         */
        public static HFilter lt(string path, HVal val) { return new Lt(Path.make(path), val); }

        /**
         * Match records which have tags less than or equals to specified value.
         * If the path is not defined then it is unmatched.
         */
        public static HFilter le(string path, HVal val) { return new Le(Path.make(path), val); }

        /**
         * Match records which have tags greater than specified value.
         * If the path is not defined then it is unmatched.
         */
        public static HFilter gt(string path, HVal val) { return new Gt(Path.make(path), val); }

        /**
         * Match records which have tags greater than or equal to specified value.
         * If the path is not defined then it is unmatched.
         */
        public static HFilter ge(string path, HVal val) { return new Ge(Path.make(path), val); }

        /**
         * Return a query which is the logical-and of this and that query.
         */
        public HFilter and(HFilter that) { return new And(this, that); }

        /**
         * Return a query which is the logical-or of this and that query.
         */
        public HFilter or(HFilter that) { return new Or(this, that); }

        //////////////////////////////////////////////////////////////////////////
        // Constructor
        //////////////////////////////////////////////////////////////////////////

        /** Package private constructor subclasses */
        private HFilter() { }

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        /* Return if given tags entity matches this query. */
        public abstract bool include(HDict dict, IPather pather);

        /** String encoding */
        public override string ToString()
        {
            if (m_filString == null)
                m_filString = toStr();
            return m_filString;
        }


        /* Used to lazily build toString */
        protected abstract string toStr();

        /** Hash code is based on string encoding */
        public int hashCode() { return ToString().GetHashCode(); }

        /** Equality is based on string encoding */
        public bool hequals(object that)
        {
            if (!(that is HFilter)) return false;
            HFilter filthat = (HFilter)that;
            return (ToString().CompareTo(filthat.ToString()) == 0);
        }

        //////////////////////////////////////////////////////////////////////////
        // HFilter.IPath
        //////////////////////////////////////////////////////////////////////////

        /** Pather is a callback interface used to resolve query paths. */
        public interface IPather
        {
            HDict find(string strRef);
        }
        /*{
            *
             * Given a HRef string identifier, resolve to an entity's
             * HDict respresentation or ref is not found return null.
             *
            HDict find(string strRef);
        }*/

        //////////////////////////////////////////////////////////////////////////
        // HFilter.Path
        //////////////////////////////////////////////////////////////////////////

        /** Path is a simple name or a complex path using the "->" separator */
        public abstract class Path
        {
            /** Construct a new Path from string or throw ParseException */
            public static Path make(string path)
            {
                try
                {
                    // optimize for common single name case
                    int dash = path.IndexOf('-');
                    if (dash < 0) return new Path1(path);

                    // parse
                    int s = 0;
                    List<string> acc = new List<string>();
                    while (true)
                    {
                        string n = path.Substring(s, dash-s);
                        if (n.Length == 0) throw new Exception();
                        acc.Add(n);
                        if (path.ElementAt(dash + 1) != '>') throw new Exception();
                        s = dash + 2;
                        dash = path.IndexOf('-', s);
                        if (dash < 0)
                        {
                            n = path.Substring(s, path.Length-s); 
                            if (n.Length == 0) throw new Exception();
                            acc.Add(n);
                            break;
                        }
                    }
                    return new PathN(path, acc.ToArray());
                }
                catch (Exception)
                { }
                throw new FormatException("Path: " + path);
            }

            /** Number of names in the path. */
            public abstract int size();

            /** Get name at given index. */
            public abstract string get(int i);

            /** Hashcode is based on string. */
            public int hashCode()
            {
                return ToString().GetHashCode();
            }

            /** Equality is based on string. */
            public bool hequals(object that)
            {
                return (ToString().CompareTo(that.ToString()) == 0);
            }

            // ToString abstract removed .NET already has a definition for ToString()
        }

        public class Path1 : Path
        {
            private string m_strName;
            public Path1(string n)
            {
                m_strName = n;
            }
            public override int size()
            {
                return 1;
            }
            public override string get(int i)
            {
                if (i == 0)
                    return m_strName;
                throw new ArgumentException("Path1 has only one element, invalid get request for element " + i.ToString(), "i");
            }
            public override string ToString()
            {
                return m_strName;
            }
            
        }

        public class PathN : Path
        {
            private string m_str;
            private string[] m_strNames;
            public PathN(string s, string [] n)
            {
                m_str = s;
                m_strNames = n;
            }
            public override int size()
            {
                return m_strNames.Length;
            }
            public override string get(int i)
            {
                return m_strNames[i];
            }
            public override string ToString()
            {
                return m_str;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // PathFilter
        //////////////////////////////////////////////////////////////////////////

        public abstract class PathFilter : HFilter
        {
            protected Path m_path;
            public PathFilter(Path p)
            {
                m_path = p;
            }
            public override bool include(HDict dict, IPather pather)
            {
                HVal val = dict.get(m_path.get(0), false);
                if (m_path.size() != 1)
                {
                    HDict nt = dict;
                    for (int i = 1; i < m_path.size(); i++)
                    {
                        if (!(val is HRef))
                        {
                            val = null;
                            break;
                        }
                        nt = pather.find(((HRef)val).ToString());
                        if (nt == null)
                        {
                            val = null;
                            break;
                        }
                        val = nt.get(m_path.get(i), false);
                    }
                }
                return doInclude(val);
            }
            public abstract bool doInclude(HVal val);
        }

        //////////////////////////////////////////////////////////////////////////
        // Has
        //////////////////////////////////////////////////////////////////////////

        public class Has : PathFilter
        {
            public Has(Path p) :base(p)
            {
                m_path = p;
            }
            public override bool doInclude(HVal v)
            {
                return (v != null);
            }
            protected override string toStr()
            {
                return m_path.ToString();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Missing
        //////////////////////////////////////////////////////////////////////////

        public class Missing : PathFilter
        {
            public Missing(Path p) : base (p)
            {
                m_path = p;
            }
            public override bool doInclude(HVal v)
            {
                return v == null;
            }
            protected override string toStr()
            {
                return "not " + m_path;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // CmpFilter
        //////////////////////////////////////////////////////////////////////////

        public abstract class CmpFilter : PathFilter
        {
            protected HVal m_val;

            public CmpFilter(Path p, HVal val) : base(p)
            {
                m_path = p;
                m_val = val;
            }
            protected override string toStr()
            {
                StringBuilder s = new StringBuilder();
                s.Append(m_path).Append(cmpStr()).Append(m_val.toZinc());
                return s.ToString();
            }
            public bool sameType(HVal v)
            {
                Type refClass = m_val.GetType();
                Type testClass = v.GetType();
                if (v == null) return false;
                if (refClass == testClass)
                    return true;
                else return false;
            }
            public abstract string cmpStr();
        }

        //////////////////////////////////////////////////////////////////////////
        // Eq
        //////////////////////////////////////////////////////////////////////////

        public class Eq : CmpFilter
        {
            public Eq(Path p, HVal v) : base (p, v)
            {
                m_path = p;
                m_val = v;
            }
            public override string cmpStr()
            {
                return "==";
            }
            public override bool doInclude(HVal v)
            {
                return v != null && v.hequals(m_val);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Ne
        //////////////////////////////////////////////////////////////////////////

        public class Ne : CmpFilter
        {
            public Ne(Path p, HVal v) : base (p, v)
            {
                m_path = p;
                m_val = v;
            }
            public override string cmpStr()
            {
                return "!=";
            }
            public override bool doInclude(HVal v)
            {
                return v != null && !v.hequals(m_val);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Lt
        //////////////////////////////////////////////////////////////////////////

        public class Lt : CmpFilter
        {
            public Lt(Path p, HVal v) : base (p, v)
            {
                m_path = p;
                m_val = v;
            }
            public override string cmpStr()
            {
                return "<";
            }
            public override bool doInclude(HVal v)
            {
                // Problem here is same type checks for null but this does not 
                //   here .NET on the compareto throws an exception (null reference)
                //  Added check for v == null
                return v != null && sameType(v) && v.CompareTo(m_val) < 0;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Le
        //////////////////////////////////////////////////////////////////////////

        public class Le : CmpFilter
        {
            public Le(Path p, HVal v) : base (p, v)
            {
                m_path = p;
                m_val = v;
            }
            public override string cmpStr()
            {
                return "<=";
            }
            public override bool doInclude(HVal v)
            {
                // Problem here is same type checks for null but this does not 
                //   here .NET on the compareto throws an exception (null reference)
                //  Added check for v == null
                return v != null && sameType(v) && v.CompareTo(m_val) <= 0;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Gt
        //////////////////////////////////////////////////////////////////////////

        public class Gt : CmpFilter
        {
            public Gt(Path p, HVal v) : base (p, v)
            {
                m_path = p;
                m_val = v;
            }
            public override string cmpStr()
            {
                return ">";
            }
            public override bool doInclude(HVal v)
            {
                // Problem here is same type checks for null but this does not 
                //   here .NET on the compareto throws an exception (null reference)
                //  Added check for v == null
                return v != null && sameType(v) && v.CompareTo(m_val) > 0;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Ge
        //////////////////////////////////////////////////////////////////////////

        public class Ge : CmpFilter
        {
            public Ge(Path p, HVal v) : base (p, v)
            {
                m_path = p;
                m_val = v;
            }
            public override string cmpStr()
            {
                return ">=";
            }
            public override bool doInclude(HVal v)
            {
                // Problem here is same type checks for null but this does not 
                //   here .NET on the compareto throws an exception (null reference)
                //  Added check for v == null
                return v != null && sameType(v) && v.CompareTo(m_val) >= 0;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Compound
        //////////////////////////////////////////////////////////////////////////

        public abstract class CompoundFilter : HFilter
        {
            protected HFilter m_a;
            protected HFilter m_b;
            public CompoundFilter(HFilter a, HFilter b)
            {
                m_a = a;
                m_b = b;
            }
            public abstract string keyword();
            protected override string toStr()
            {
                /* This part has no relevance - deep is never used
                bool deep = false;
                if ((m_a is CompoundFilter) || (m_b is CompoundFilter))
                    deep = true;
                */
                StringBuilder s = new StringBuilder();
                if (m_a is CompoundFilter)
                    s.Append('(').Append(m_a).Append(')');
                else s.Append(m_a);
                s.Append(' ').Append(keyword()).Append(' ');
                if (m_b is CompoundFilter) s.Append('(').Append(m_b).Append(')');
                else s.Append(m_b);
                return s.ToString();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // And
        //////////////////////////////////////////////////////////////////////////

        public class And : CompoundFilter
        {
            public And(HFilter a, HFilter b) : base (a, b)
            {
                m_a = a;
                m_b = b;
            }
            public override string keyword()
            {
                return "and";
            }
            public override bool include(HDict dict, IPather pather)
            {
                return m_a.include(dict, pather) && m_b.include(dict, pather);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Or
        //////////////////////////////////////////////////////////////////////////

        public class Or : CompoundFilter
        {
            public Or(HFilter a, HFilter b) : base (a, b)
            {
                m_a = a;
                m_b = b;
            }

            public override string keyword()
            {
                return "or";
            }
            public override bool include(HDict dict, IPather pather)
            {
                return (m_a.include(dict, pather) || m_b.include(dict, pather));
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // FilterParser
        //////////////////////////////////////////////////////////////////////////

        public class FilterParser
        {
            private HaystackTokenizer m_tokenizer;
            private HaystackToken m_tokCur; // cur
            private object m_curVal;
            private HaystackToken m_tokPeek; // peek
            private object m_peekVal;

            public FilterParser(string strIn)
            {
                MemoryStream msRdr = new MemoryStream(Encoding.UTF8.GetBytes(strIn));
                m_tokenizer = new HaystackTokenizer(new StreamReader(msRdr));
                consume();
                consume();
            }

            public HFilter parse()
            {
                HFilter f = condOr();
                verify(HaystackToken.eof);
                return f;
            }

            private HFilter condOr()
            {
                HFilter lhs = condAnd();
                if (!isKeyword("or")) return lhs;
                consume();
                return lhs.or(condOr());
            }

            private HFilter condAnd()
            {
                HFilter lhs = term();
                if (!isKeyword("and")) return lhs;
                consume();
                return lhs.and(condAnd());
            }

            private HFilter term()
            {
                if (m_tokCur == HaystackToken.lparen)
                {
                    consume();
                    HFilter f = condOr();
                    consume(HaystackToken.rparen);
                    return f;
                }

                if (isKeyword("not") && m_tokPeek == HaystackToken.id)
                {
                    consume();
                    return new Missing(path());
                }

                Path p = path();
                if (m_tokCur == HaystackToken.eq) { consume(); return new Eq(p, val()); }
                if (m_tokCur == HaystackToken.notEq) { consume(); return new Ne(p, val()); }
                if (m_tokCur == HaystackToken.lt) { consume(); return new Lt(p, val()); }
                if (m_tokCur == HaystackToken.ltEq) { consume(); return new Le(p, val()); }
                if (m_tokCur == HaystackToken.gt) { consume(); return new Gt(p, val()); }
                if (m_tokCur == HaystackToken.gtEq) { consume(); return new Ge(p, val()); }

                return new Has(p);
            }

            private Path path()
            {
                string id = pathName();
                if (m_tokCur != HaystackToken.arrow)
                    return Path1.make(id);

                List<string> segments = new List<string>();
                segments.Add(id);
                StringBuilder s = new StringBuilder().Append(id);
                while (m_tokCur == HaystackToken.arrow)
                {
                    consume(HaystackToken.arrow);
                    id = pathName();
                    segments.Add(id);
                    s.Append(HaystackToken.arrow).Append(id);
                }
                return new PathN(s.ToString(), segments.ToArray());
            }

            private string pathName()
            {
                if (m_tokCur != HaystackToken.id)
                    throw err("Expecting tag name, not " + curToStr());
                // Not to sure if (string)curval is the same as curval.toString() in Java - but it is not the same in .NET
                string id = (string)m_curVal;
                consume();
                return id;
            }

            private HVal val()
            {
                if (m_tokCur.Literal)
                {
                    HVal val = (HVal)m_curVal;
                    consume();
                    return val;
                }

                if (m_tokCur == HaystackToken.id)
                {
                    // NOTE: once again we are assuming m_curVal is a string
                    if ("true".Equals(m_curVal)) { consume(); return HBool.TRUE; }
                    if ("false".Equals(m_curVal)) { consume(); return HBool.FALSE; }
                }

                throw err("Expecting value literal, not " + curToStr());
            }

            private bool isKeyword(string n)
            {
                // NOTE: once again we are assuming m_curVal is a string
                return m_tokCur == HaystackToken.id && n.Equals(m_curVal);
            }

            private void verify(HaystackToken expected)
            {
                if (m_tokCur != expected)
                    throw err("Expected " + expected + " not " + curToStr());
            }

            private string curToStr()
            {
                return m_curVal != null ? "" + m_tokCur + " " + m_curVal : m_tokCur.ToString();
            }

            private void consume()
            {
                consume(null);
            }
            private void consume(HaystackToken expected)
            {
                if (expected != null) verify(expected);
                m_tokCur = m_tokPeek;
                m_curVal = m_peekVal;
                m_tokPeek = m_tokenizer.next();
                m_peekVal = m_tokenizer.Val;
            }
            // Kept for sake of making it as close to Java toolkit but fail to see the value it adds
            private FormatException err(string msg) { return new FormatException(msg); }


        }    
    }
}
