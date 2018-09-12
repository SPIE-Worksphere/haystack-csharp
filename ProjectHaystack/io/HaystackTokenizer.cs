//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   22 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//   18 Aug 2018 Ian Davies This does not pass Unit tests for HBoolTest.testZinc - problem is the ZincReader and 
//                          Tokenizer pass over the one character zinc before it is tokenised and used.
//                          Changed logic to get the variables initialised without moving past the required tokens
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.io
{
    public class HaystackTokenizer : IDisposable
    {
        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////

        private HaystackToken m_tok; // current token type
        private object m_val;        //token literal or identifier
        private int m_iLine = 1;             // current line number

        private StreamReader m_srIn;   // underlying stream
        private int m_cCur;           // current char
        private int m_cPeek;          // next char
        private bool m_bInitial;
        private const int m_iEOF = -1;
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////
        public object Val { get { return m_val; } }
        public int Line { get { return m_iLine; } }
        public HaystackToken Token { get { return m_tok; } }
            
        //////////////////////////////////////////////////////////////////////////
        // IDisposable
        //////////////////////////////////////////////////////////////////////////
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    m_srIn.Dispose();
                    m_srIn = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HaystackTokenizer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
        //////////////////////////////////////////////////////////////////////////
        // Construction
        //////////////////////////////////////////////////////////////////////////

        public HaystackTokenizer(StreamReader instrm)
        {
            m_srIn = instrm;
            m_tok = HaystackToken.eof;
            m_bInitial = true;
            //consume();
            //consume();
        }
        // Changed to IDisposable 
        public void close()
        {
            Dispose();
        }

        //////////////////////////////////////////////////////////////////////////
        // Tokenizing
        //////////////////////////////////////////////////////////////////////////

        public HaystackToken next()
        {
            if (m_bInitial)
            {
                consume();
                consume();
                m_bInitial = false;
            }
            // reset
            m_val = null;

            // skip non-meaningful whitespace and comments
            int startLine = m_iLine;
            while (true) // This is valid but not how I would implement a loop - may change to something more robust later
            {
                // treat space, tab, non-breaking space as whitespace
                if (m_cCur != m_iEOF)
                {
                    if ((char)m_cCur == ' ' || (char)m_cCur == '\t' || (char)m_cCur == 0xa0)
                    {
                        consume();
                        continue;
                    }

                    // comments
                    if ((char)m_cCur == '/')
                    {
                        if ((char)m_cPeek == '/') { skipCommentsSL(); continue; }
                        if ((char)m_cPeek == '*') { skipCommentsML(); continue; }
                    }
                }
                break;
            }
            if (m_cCur != m_iEOF)
            {
                // newlines
                if ((char)m_cCur == '\n' || (char)m_cCur == '\r')
                {
                    if ((char)m_cCur == '\r' && (char)m_cPeek == '\n') consume('\r');
                    consume();
                    ++m_iLine;
                    return m_tok = HaystackToken.nl;
                }
            }
            //if ((m_bCurInit) && (m_bEOF)) return m_tok = HaystackToken.eof;
            // handle various starting chars
            if (m_cCur >= 0)
            {
                if (isIdStart(m_cCur)) return m_tok = id();
                if (m_cCur == '"') return m_tok = str();
                if (m_cCur == '@') return m_tok = refh();
                if (isDigit(m_cCur)) return m_tok = num();
                if (m_cCur == '`') return m_tok = uri();
                if (m_cCur == '-' && isDigit(m_cPeek)) return m_tok = num();
            }

            return m_tok = symbol();
        }

        //////////////////////////////////////////////////////////////////////////
        // Token Productions
        //////////////////////////////////////////////////////////////////////////

        private HaystackToken id()
        {
            StringBuilder s = new StringBuilder();
            while (isIdPart(m_cCur))
            {
                s.Append((char)m_cCur);
                consume();
            }
            m_val = s.ToString();
            return HaystackToken.id;
        }

        private static bool isIdStart(int cur)
        {
            if (cur < 0) return false;
            if ('a' <= (char)cur && (char)cur <= 'z') return true;
            if ('A' <= (char)cur && (char)cur <= 'Z') return true;
            return false;
        }

        private static bool isIdPart(int cur)
        {
            if (cur < 0) return false;
            if (isIdStart(cur)) return true;
            if (isDigit(cur)) return true;
            if ((char)cur == '_') return true;
            return false;
        }

        private static bool isDigit(int cur)
        {
            if (cur < 0) return false;
            return char.IsNumber((char)cur);
            //return '0' >= cur && cur <= '9';
        }

        private HaystackToken num()
        {
            string strDecNumSep = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            char cNumSep = '.';
            if (strDecNumSep.Length == 1)
                cNumSep = strDecNumSep[0];
            bool bHex = false;
            if (m_cCur < 0) err("unexpected eof in num");
            if (!(m_cCur < 0 || m_cPeek < 0))
            {
                // hex number (no unit allowed)
                bHex = (char)m_cCur == '0' && (char)m_cPeek == 'x';
            }
            bool bExit = false;
            if (bHex)
            {
                consume('0');
                consume('x');
                StringBuilder sb01 = new StringBuilder();
                bExit = false;
                while (!bExit)
                {
                    if (HaystackTokenizer.isHex(m_cCur))
                    {
                        sb01.Append((char)m_cCur);
                        consume();
                    }
                    else if (m_cCur == '_')
                        consume();
                    else
                        bExit = true;
                }
                m_val = HNum.make(Int64.Parse(sb01.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier)); 
                return HaystackToken.num;
            }
            // consume all things that might be part of this number token
            StringBuilder s = new StringBuilder().Append((char)m_cCur);
            consume();
            int colons = 0;
            int dashes = 0;
            int unitIndex = 0;
            bool exp = false;
            bExit = false;
            while (!bExit)
            {
                if (m_cCur >= 0) // Check for eof
                {
                    if (!char.IsDigit((char)m_cCur))
                    {
                        if (exp && (m_cCur == '+' || m_cCur == '-')) { }
                        else if (m_cCur == '-') { ++dashes; }
                        else if ((m_cPeek >= 0) && (m_cCur == ':') && (char.IsDigit((char)m_cPeek)))
                        {
                            ++colons;
                        }
                        else if ((exp || colons >= 1) && m_cCur == '+') { }
                        else if (m_cCur == cNumSep)
                        {
                            if (m_cPeek >= 0)
                            {
                                if (!char.IsDigit((char)m_cPeek)) break;
                            }
                        }
                        else if ((m_cPeek >= 0) && (((char)m_cCur == 'e' || (char)m_cCur == 'E') && ((char)m_cPeek == '-' || (char)m_cPeek == '+' || char.IsDigit((char)m_cPeek))))
                        {
                            exp = true;
                        }
                        else if (char.IsLetter((char)m_cCur) || m_cCur == '%' || m_cCur == '$' || m_cCur == '/' || m_cCur > 128) { if (unitIndex == 0) unitIndex = s.Length; }
                        else if ((m_cPeek >= 0) && (m_cCur == '_'))
                        {
                            if (unitIndex == 0 && char.IsDigit((char)m_cPeek))
                            {
                                consume();
                                continue;
                            }
                            else
                            {
                                if (unitIndex == 0)
                                    unitIndex = s.Length;
                            }
                        }
                        else
                        {
                            bExit = true;
                        }
                    }
                }
                else bExit = true;
                if (!bExit)
                {
                    s.Append((char)m_cCur);
                    consume();
                }
            }

            if (dashes == 2 && colons == 0) return date(s.ToString());
            if (dashes == 0 && colons >= 1) return time(s, colons == 1);
            if (dashes >= 2) return dateTime(s);
            return number(s.ToString(), unitIndex);
        }

        private static bool isHex(int cur)
        {
            if (cur < 0) return false;
            cur = char.ToLower((char)cur);
            if ('a' <= cur && cur <= 'f') return true;
            if (isDigit(cur)) return true;
            return false;
        }

        private HaystackToken date(string s)
        {
            // Java use to bubble Parse exception as err - this just accepts that the debug printing
            //  will be done by the user of this toolkit.
            m_val = HDate.make(s);
            return HaystackToken.date;
        }

        // we don't require hour to be two digits and we don't require seconds 
        private HaystackToken time(StringBuilder s, bool addSeconds)
        {
            // Java use to bubble Parse exception as err - this just accepts that the debug printing
            //  will be done by the user of this toolkit.
            if (s[1] == ':') s.Insert(0, '0');
            if (addSeconds) s.Append(":00");
            m_val = HTime.make(s.ToString());
            return HaystackToken.time;
        }

        private HaystackToken dateTime(StringBuilder s)
        {
            bool bFlag1 = true;
            // xxx timezone
            if ((m_cCur < 0 || m_cPeek < 0) || ((char)m_cCur != ' ' || !char.IsUpper((char)m_cPeek)))
            {
                if (s[s.Length - 1] == 'Z') s.Append(" UTC");
                else err("Expecting timezone");
                bFlag1 = false;
            }
            if (bFlag1)
            {
                consume();
                s.Append(' ');
                while (isIdPart(m_cCur)) { s.Append((char)m_cCur); consume(); }

                // handle GMT+xx or GMT-xx
                if ((m_cCur == '+' || m_cCur == '-') && s.ToString().EndsWith("GMT"))
                {
                    s.Append((char)m_cCur); consume();
                    while (isDigit(m_cCur)) { s.Append((char)m_cCur); consume(); }
                }
            }
            // Java use to bubble Parse exception as err - this just accepts that the debug printing
            //  will be done by the user of this toolkit.
            m_val = HDateTime.make(s.ToString(), true);
            return HaystackToken.dateTime;
        }

        private HaystackToken number(string s, int unitIndex)
        {
            try
            {
                if (unitIndex == 0)
                {
                    m_val = HNum.make(Double.Parse(s));
                }
                else
                {
                    string doubleStr = s.Substring(0, unitIndex);
                    string unitStr = s.Substring(unitIndex);
                    m_val = HNum.make(Double.Parse(doubleStr), unitStr);
                }
            }
            catch (Exception)
            {
                err("[HaystackTokenizer::number]Invalid Number literal: " + s);
            }
            return HaystackToken.num;
        }

        private HaystackToken str()
        {
            consume('"');
            StringBuilder s = new StringBuilder();
            while (true)
            {
                if (m_cCur == m_iEOF) err("Unexpected end of str");
                if ((char)m_cCur == '"') { consume('"'); break; }
                if ((char)m_cCur == '\\') { s.Append(escape()); continue; }
                s.Append((char)m_cCur);
                consume();
            }
            m_val = HStr.make(s.ToString());
            return HaystackToken.str;
        }

        private HaystackToken refh()
        {
            if (m_cCur < 0) err("Unexpected eof in refh");
            consume('@');
            StringBuilder s = new StringBuilder();
            while (true)
            {
                if (HRef.isIdChar(m_cCur))
                {
                    s.Append((char)m_cCur);
                    consume();
                }
                else
                {
                    break;
                }
            }
            m_val = HRef.make(s.ToString(), null);
            return HaystackToken.refh;
        }

        private HaystackToken uri()
        {
            if (m_cCur < 0) err("Unexpected end of uri");
            consume('`');
            StringBuilder s = new StringBuilder();
            while (true)
            {
                if ((m_cCur < (int)char.MinValue) || (m_cCur > (int)char.MaxValue))
                    err("Unexpected character in uri at a value of " + m_cCur.ToString());
                if ((char)m_cCur == '`')
                {
                    consume('`');
                    break;
                }
                if (m_cCur == m_iEOF || (char)m_cCur == '\n') err("Unexpected end of uri");
                if ((char)m_cCur == '\\')
                {
                    switch ((char)m_cPeek)
                    {
                        case ':':
                        case '/':
                        case '?':
                        case '#':
                        case '[':
                        case ']':
                        case '@':
                        case '\\':
                        case '&':
                        case '=':
                        case ';':
                            s.Append((char)m_cCur);
                            s.Append((char)m_cPeek);
                            consume();
                            consume();
                            break;
                        default:
                            s.Append(escape());
                            break;
                    }
                }
                else
                {
                    s.Append((char)(m_cCur));
                    consume();
                }
            }
            m_val = HUri.make(s.ToString());
            return HaystackToken.uri;
        }

        private char escape()
        {
            if (m_cCur < 0) err("unexpected eof in escape");
            consume('\\');
            switch ((char)m_cCur)
            {
                case 'b': consume(); return '\b';
                case 'f': consume(); return '\f';
                case 'n': consume(); return '\n';
                case 'r': consume(); return '\r';
                case 't': consume(); return '\t';
                case '"': consume(); return '"';
                case '$': consume(); return '$';
                case '\'': consume(); return '\'';
                case '`': consume(); return '`';
                case '\\': consume(); return '\\';
            }

            // check for uxxxx
            StringBuilder esc = new StringBuilder();
            if ((char)m_cCur == 'u')
            {
                consume('u');
                esc.Append((char)m_cCur); consume();
                esc.Append((char)m_cCur); consume();
                esc.Append((char)m_cCur); consume();
                esc.Append((char)m_cCur); consume();
                try
                {
                    // Allows for hexadecimal - UTF 16
                    return Convert.ToChar((Int16.Parse(esc.ToString(), System.Globalization.NumberStyles.AllowHexSpecifier)));
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Invalid unicode escape: " + esc.ToString(), "esc");
                }
                catch (OverflowException)
                {
                    throw new ArgumentException("Invalid unicode escape: " + esc.ToString(), "esc");
                }
                catch (Exception genexcp)
                {
                    throw new Exception("Invalid unicode escape: " + esc.ToString() + ", general exception: " + genexcp.Message);
                }
            }
            err("Invalid escape sequence: " + (char)m_cCur);
            return (char)0x00; // this code will never execute because err throws an exception
        }

        private HaystackToken symbol()
        {
            if (m_cCur == m_iEOF) return HaystackToken.eof;
            int c = m_cCur;
            consume();
            switch ((char)c)
            {
                case ',':
                    return HaystackToken.comma;
                case ':':
                    return HaystackToken.colon;
                case ';':
                    return HaystackToken.semicolon;
                case '[':
                    return HaystackToken.lbracket;
                case ']':
                    return HaystackToken.rbracket;
                case '{':
                    return HaystackToken.lbrace;
                case '}':
                    return HaystackToken.rbrace;
                case '(':
                    return HaystackToken.lparen;
                case ')':
                    return HaystackToken.rparen;
                case '<':
                    if ((char)m_cCur == '<') { consume('<'); return HaystackToken.lt2; }
                    if ((char)m_cCur == '=') { consume('='); return HaystackToken.ltEq; }
                    return HaystackToken.lt;
                case '>':
                    if ((char)m_cCur == '>') { consume('>'); return HaystackToken.gt2; }
                    if ((char)m_cCur == '=') { consume('='); return HaystackToken.gtEq; }
                    return HaystackToken.gt;
                case '-':
                    if ((char)m_cCur == '>') { consume('>'); return HaystackToken.arrow; }
                    return HaystackToken.minus;
                case '=':
                    if ((char)m_cCur == '=') { consume('='); return HaystackToken.eq; }
                    return HaystackToken.assign;
                case '!':
                    if ((char)m_cCur == '=') { consume('='); return HaystackToken.notEq; }
                    return HaystackToken.bang;
                case '/':
                    return HaystackToken.slash;
            }
            if (m_cCur == m_iEOF) return HaystackToken.eof;
            err("Unexpected symbol: '" + (char)c + "' (0x" + c.ToString("x2") + ")");
            return null; // this code will never execute because err throws an exception
        }

        //////////////////////////////////////////////////////////////////////////
        // Comments
        //////////////////////////////////////////////////////////////////////////

        private void skipCommentsSL()
        {
            consume('/');
            consume('/');
            bool bExit = false;
            while (!bExit)
            {
                if (m_cCur == '\n' || m_cCur == m_iEOF)
                    bExit = true;
                else
                    consume();
            }
        }

        private void skipCommentsML()
        {
            consume('/');
            consume('*');
            int depth = 1;
            bool bExit = false;
            while (!bExit)
            {
                if ((char)m_cCur == '*' && (char)m_cPeek == '/') { consume('*'); consume('/'); depth--; if (depth <= 0) break; }
                if ((char)m_cCur == '/' && (char)m_cPeek == '*') { consume('/'); consume('*'); depth++; continue; }
                if ((char)m_cCur == '\n') ++m_iLine;
                if (m_cCur == m_iEOF)
                {
                    err("Multi-line comment not closed");
                    bExit = true; // This will never execute because err throws an exception
                }
                else
                    consume();
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Error Handling
        //////////////////////////////////////////////////////////////////////////

        private void err(string msg)
        {
            throw new FormatException(msg + " [line " + m_iLine + "]");
        }

        //////////////////////////////////////////////////////////////////////////
        // Char
        //////////////////////////////////////////////////////////////////////////

        private void consume(int expected)
        {
            if (m_cCur != expected) err("Expected " + (char)expected);
            consume();
        }
        /// <summary>
        /// consume - this departs from the java haystack version and will only
        /// ever go to peek at eof.  This prevents single length streams from
        /// initialising and loosing the token.
        /// </summary>
        private void consume()
        {
            try
            {
                m_cCur = m_cPeek;
                m_cPeek = m_srIn.Read();
                if (m_cPeek < 0) // End of stream
                {
                    m_cPeek = m_iEOF;
                }
            }
            catch (IOException)
            {
                m_cPeek = m_iEOF;
            }
        }
    }
}
