//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   13 July 2018   Ian Davies  Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.io
{
    /**
     * HZincReader reads grids using the Zinc format.
     *
     * @see <a href='http://project-haystack.org/doc/Zinc'>Project Haystack</a>
     */
    public class HZincReader : HGridReader
    {
        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////

        private HaystackTokenizer m_tokenizer;
        private Stream m_instrm;

        private HaystackToken m_tokCur;
        private object m_curVal;
        private int m_icurLine;

        private HaystackToken m_tokPeek;
        private object m_peekVal;
        private int m_ipeekLine;

        private int m_iversion;
        //private bool m_bisTop;

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////
        public override Stream BaseStream { get { return m_instrm; } }

        //////////////////////////////////////////////////////////////////////////
        // Construction
        //////////////////////////////////////////////////////////////////////////

        // Read from UTF-8 input stream. 
        public HZincReader(Stream instrm)
        {
            try
            {
                m_instrm = instrm;
                m_tokenizer = new HaystackTokenizer(new StreamReader(instrm, Encoding.UTF8));
                init();
            }
            catch (IOException e)
            {
                err("init failed", e);
            }
        }
        // Read from in-memory string as UTF8 characters. 
        public HZincReader(string instr)
        {
            MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(instr));
            m_instrm = mStrm;
            m_tokenizer = new HaystackTokenizer(new StreamReader(mStrm));
            init();
        }

        private void init()
        {
            m_iversion = 3;
            //m_bisTop = true;
            consume();
            consume();
        }
        
        //////////////////////////////////////////////////////////////////////////
        // Public
        //////////////////////////////////////////////////////////////////////////

        // Close underlying input stream 
        public void close()
        {
            m_tokenizer.close();
        }


        // Read a value and auto-close the stream 
        public HVal readVal()
        {
            return readVal(true);
        }

        // Read a value. Close the stream if close is true 
        public HVal readVal(bool close)
        {
            try
            {
                HVal val = null;
                bool bValisVer = false;
                if (m_curVal is string)
                {
                    if (((string)m_curVal).CompareTo("ver") == 0)
                        bValisVer = true;
                }
                if (m_tokCur == HaystackToken.id && bValisVer)
                    val = parseGrid();
                else
                    val = parseVal();
                // Depart from Java - 15.08.2018 after testing with Java Toolkit
                // It is possible there might be newlines or other non prihtables before eof
                // only verify it is not a token of interest
                bool bEnd = false;
                while (!bEnd)
                {
                    if (tryVerify(HaystackToken.nl, false))
                        consume();
                    else
                    {
                        // This will throw an exception if it is not eof
                        verify(HaystackToken.eof);
                        bEnd = true;
                    }
                }
                return val;
            }
            finally
            {
                if (close) this.close();
            }

        }

        // Convenience for {@link #readVal} as Grid 
        public override HGrid readGrid()
        {
            return (HGrid)readVal(true);
        }

        // Read a list of grids separated by blank line from stream 
        public HGrid[] readGrids()
        {
            List<HGrid> acc = new List<HGrid>();
            while (m_tokCur == HaystackToken.id)
                acc.Add(parseGrid());
            return acc.ToArray();
        }

        /**
         * Read a set of tags as {@code name:val} pairs separated by space. The
         * tags may optionally be surrounded by '{' and '}'
         */
        public HDict readDict()
        {
            return parseDict();
        }

        /**
         * Read scalar value: Bool, Int, Str, Uri, etc
         *
         * @deprecated Will be removed in future release.
         */
        public HVal readScalar()
        {
            return parseVal();
        }

        //////////////////////////////////////////////////////////////////////////
        // Utils
        //////////////////////////////////////////////////////////////////////////

        private HVal parseVal()
        {
            // if it's an id
            if (m_tokCur == HaystackToken.id)
            {
                string id = (string)m_curVal;
                consume(HaystackToken.id);

                // check for coord or xstr
                if (m_tokCur == HaystackToken.lparen)
                {
                    if (m_tokPeek == HaystackToken.num)
                        return parseCoord(id);
                    else
                        return parseXStr(id);
                }

                // check for keyword
                if ("T".CompareTo(id) == 0) return HBool.TRUE;
                if ("F".CompareTo(id) == 0) return HBool.FALSE;
                if ("N".CompareTo(id) == 0) return null;
                if ("M".CompareTo(id) == 0) return HMarker.VAL;
                if ("NA".CompareTo(id) == 0) return HNA.VAL;
                if ("R".CompareTo(id) == 0) return HRemove.VAL;
                if ("NaN".CompareTo(id) == 0) return HNum.NaN;
                if ("INF".CompareTo(id) == 0) return HNum.POS_INF;
                if (id.StartsWith("^")) return HStr.make(id);

                err("Unexpected identifier: " + id);
                return null; // This is not code that will be executable because of call to err
            }

            // literals
            if (m_tokCur.Literal) return parseLiteral();
            bool bPeekIsINF = false;
            if (m_peekVal is string)
            {
                if (((string)m_peekVal).CompareTo("INF") == 0)
                    bPeekIsINF = true;
            }
            // -INF
            if (m_tokCur == HaystackToken.minus && bPeekIsINF)
            {
                consume(HaystackToken.minus);
                consume(HaystackToken.id);
                return HNum.NEG_INF;
            }

            // nested collections
            if (m_tokCur == HaystackToken.lbracket) return parseList();
            if (m_tokCur == HaystackToken.lbrace) return parseDict();
            if (m_tokCur == HaystackToken.lt2) return parseGrid();

            err("Unexpected token: " + curToStr());
            return null; // This is not code that will be executable because of call to err
        }

        private HCoord parseCoord(string id)
        {

            if ("C".CompareTo(id) != 0)
                err("Expecting 'C' for coord, not " + id);
            consume(HaystackToken.lparen);
            HNum lat = consumeNum();
            consume(HaystackToken.comma);
            HNum lng = consumeNum();
            consume(HaystackToken.rparen);
            return HCoord.make(lat.doubleval, lng.doubleval);
        }

        private HVal parseXStr(string id)
        {
            if (!char.IsUpper(id[0]))
                err("Invalid XStr type: " + id);
            consume(HaystackToken.lparen);
            if (m_iversion < 3 && ("Bin".CompareTo(id) == 0)) return parseBinObsolete();
            string val = consumeStr();
            consume(HaystackToken.rparen);
            return HXStr.decode(id, val);
        }

        private HBin parseBinObsolete()
        {
            StringBuilder s = new StringBuilder();
            while (m_tokCur != HaystackToken.rparen && m_tokCur != HaystackToken.eof)
            {
                if (m_curVal == null) s.Append(m_tokCur.Symbol);
                else s.Append(m_curVal);
                consume();
            }
            consume(HaystackToken.rparen);
            return HBin.make(s.ToString());
        }

        private HVal parseLiteral()
        {
            object val = m_curVal;
            if (m_tokCur == HaystackToken.refh && m_tokPeek == HaystackToken.str)
            {
                val = HRef.make(((HRef)val).val, ((HStr)m_peekVal).Value);
                consume(HaystackToken.refh);
            }
            consume();
            return (HVal)val;
        }

        private HList parseList()
        {
            List<HVal> arr = new List<HVal>();
            consume(HaystackToken.lbracket);
            while (m_tokCur != HaystackToken.rbracket && m_tokCur != HaystackToken.eof)
            {
                HVal val = parseVal();
                arr.Add(val);
                if (m_tokCur != HaystackToken.comma)
                    break;
                consume(HaystackToken.comma);
            }
            consume(HaystackToken.rbracket);
            return HList.make(arr);
        }

        private HDict parseDict()
        {
            HDictBuilder db = new HDictBuilder();
            bool braces = m_tokCur == HaystackToken.lbrace;
            if (braces) consume(HaystackToken.lbrace);
            while (m_tokCur == HaystackToken.id)
            {
                // tag name
                string id = consumeTagName();
                if (!char.IsLower(id[0]))
                    err("Invalid dict tag name: " + id);

                // tag value
                HVal val = HMarker.VAL;
                if (m_tokCur == HaystackToken.colon)
                {
                    consume(HaystackToken.colon);
                    val = parseVal();
                }
                db.add(id, val);
            }
            if (braces) consume(HaystackToken.rbrace);
            return db.toDict();
        }

        private HGrid parseGrid()
        {
            bool nested = m_tokCur == HaystackToken.lt2;
            if (nested)
            {
                consume(HaystackToken.lt2);
                if (m_tokCur == HaystackToken.nl)
                    consume(HaystackToken.nl);
            }

            bool bValisVer = false;
            if (m_curVal is string)
            {
                if (((string)m_curVal).CompareTo("ver") == 0)
                    bValisVer = true;
            }
            // ver:"3.0"
            if (m_tokCur != HaystackToken.id || !bValisVer)
                err("Expecting grid 'ver' identifier, not " + curToStr());
            consume();
            consume(HaystackToken.colon);
            m_iversion = checkVersion(consumeStr());

            // grid meta
            HGridBuilder gb = new HGridBuilder();
            if (m_tokCur == HaystackToken.id)
                gb.Meta.add(parseDict());
            consume(HaystackToken.nl);

            // column definitions
            int numCols = 0;
            while (m_tokCur == HaystackToken.id)
            {
                ++numCols;
                string name = consumeTagName();
                HDict colMeta = HDict.Empty;
                if (m_tokCur == HaystackToken.id)
                    colMeta = parseDict();
                gb.addCol(name).add(colMeta);
                if (m_tokCur != HaystackToken.comma)
                    break;
                consume(HaystackToken.comma);
            }
            if (numCols == 0)
                err("No columns defined");
            consume(HaystackToken.nl);

            // grid rows
            while (true)
            {
                if (m_tokCur == HaystackToken.nl) break;
                if (m_tokCur == HaystackToken.eof) break;
                if (nested && m_tokCur == HaystackToken.gt2) break;

                // read cells
                HVal[] cells = new HVal[numCols];
                for (int i = 0; i < numCols; ++i)
                {
                    if (m_tokCur == HaystackToken.comma || m_tokCur == HaystackToken.nl || m_tokCur == HaystackToken.eof)
                        cells[i] = null;
                    else
                        cells[i] = parseVal();
                    if (i + 1 < numCols) consume(HaystackToken.comma);
                }
                gb.addRow(cells);

                // newline or end
                if (nested && m_tokCur == HaystackToken.gt2) break;
                if (m_tokCur == HaystackToken.eof) break;
                consume(HaystackToken.nl);
            }

            if (m_tokCur == HaystackToken.nl) consume(HaystackToken.nl);
            if (nested) consume(HaystackToken.gt2);
            return gb.toGrid();
        }

        private int checkVersion(string s)
        {
            if ("3.0".CompareTo(s) == 0) return 3;
            if ("2.0".CompareTo(s) == 0) return 2;
            err("Unsupported version " + s);
            return -1; // This code will not be executable due to err
        }

        //////////////////////////////////////////////////////////////////////////
        // Token Reads
        //////////////////////////////////////////////////////////////////////////

        private string consumeTagName()
        {
            verify(HaystackToken.id);
            string id = (string)m_curVal;
            if (id.Length == 0 || !char.IsLower(id[0]))
                err("Invalid dict tag name: " + id);
            consume(HaystackToken.id);
            return id;
        }

        private HNum consumeNum()
        {
            verify(HaystackToken.num);
            HNum num = (HNum)m_curVal;
            consume(HaystackToken.num);
            return num;
        }

        private string consumeStr()
        {
            verify(HaystackToken.str);
            String val = ((HStr)m_curVal).Value;
            consume(HaystackToken.str);
            return val;
        }

        private void verify(HaystackToken expected)
        {
            HaystackToken tokToCheck = m_tokCur;
            if (expected == HaystackToken.eof) // eof will stop at peek
                tokToCheck = m_tokPeek;
            if (tokToCheck != expected)
                err("Expected " + expected + " not " + curToStr());
        }

        private bool tryVerify(HaystackToken expected, bool bErr)
        {
            HaystackToken tokToCheck = m_tokCur;
            if (expected == HaystackToken.eof) // eof will stop at peek
                tokToCheck = m_tokPeek;
            if (tokToCheck != expected)
            {
                if (bErr)
                    err("Expected " + expected + " not " + curToStr());
                return false;
            }
            else return true;
        }
        private string curToStr()
        {
            return m_curVal != null ? m_tokCur + " " + m_curVal : m_tokCur.ToString();
        }

        private void consume() { consume(null); }
        private void consume(HaystackToken expected)
        {
            if (expected != null) verify(expected);
            
            m_tokCur = m_tokPeek;
            m_curVal = m_peekVal;
            m_icurLine = m_ipeekLine;

            m_tokPeek = m_tokenizer.next();
            m_peekVal = m_tokenizer.Val;
            m_ipeekLine = m_tokenizer.Line;
        }

        private void err(string msg)
        {
            err(msg, null);
        }
        private void err(string msg, Exception e)
        {
            if (e != null)
                throw new Exception(msg + " [line " + m_icurLine + "]", e);
            else
                throw new Exception(msg + " [line " + m_icurLine + "]");

        }
    }
}
