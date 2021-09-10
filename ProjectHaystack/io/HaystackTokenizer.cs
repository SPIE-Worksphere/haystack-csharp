using System;
using System.Globalization;
using System.IO;
using System.Text;
using ProjectHaystack.Validation;

namespace ProjectHaystack.io
{
    public class HaystackTokenizer : IDisposable
    {
        private StreamReader _sourceReader;

        private int _currentLineNumber = 1;
        private HaystackToken _currentToken = HaystackToken.eof;
        private object _currentValue;
        private int _currentChar;

        private int _peekChar;

        private bool _atStart = true;
        private const int _endOfFile = -1;

        private NumberFormatInfo _numberFormat = CultureInfo.InvariantCulture.NumberFormat;

        public HaystackTokenizer(StreamReader reader)
        {
            _sourceReader = reader;
        }

        public object Val => _currentValue;
        public int LineNumber => _currentLineNumber;
        public HaystackToken Token => _currentToken;

        public void Dispose()
        {
            if (_sourceReader != null)
            {
                var reader = _sourceReader;
                _sourceReader = null;
                reader.Dispose();
            }
        }

        public HaystackToken Next()
        {
            if (_atStart)
            {
                consume();
                consume();
                _atStart = false;
            }
            // reset
            _currentValue = null;

            // skip non-meaningful whitespace and comments
            while (true) // This is valid but not how I would implement a loop - may change to something more robust later
            {
                // treat space, tab, non-breaking space as whitespace
                if (_currentChar != _endOfFile)
                {
                    if ((char)_currentChar == ' ' || (char)_currentChar == '\t' || (char)_currentChar == 0xa0)
                    {
                        consume();
                        continue;
                    }

                    // comments
                    if ((char)_currentChar == '/')
                    {
                        if ((char)_peekChar == '/') { skipCommentsSL(); continue; }
                        if ((char)_peekChar == '*') { skipCommentsML(); continue; }
                    }
                }
                break;
            }

            if (_currentChar != _endOfFile)
            {
                // newlines
                if ((char)_currentChar == '\n' || (char)_currentChar == '\r')
                {
                    if ((char)_currentChar == '\r' && (char)_peekChar == '\n') consume('\r');
                    consume();
                    ++_currentLineNumber;
                    return _currentToken = HaystackToken.nl;
                }
            }

            // handle various starting chars
            if (_currentChar >= 0)
            {
                if (isIdStart(_currentChar)) return _currentToken = ReadId();
                if (_currentChar == '"') return _currentToken = ReadString();
                if (_currentChar == '@') return _currentToken = ReadReference();
                if (isDigit(_currentChar)) return _currentToken = ReadNumber();
                if (_currentChar == '`') return _currentToken = ReadUri();
                if (_currentChar == '-' && isDigit(_peekChar)) return _currentToken = ReadNumber();
            }

            return _currentToken = symbol();
        }

        //////////////////////////////////////////////////////////////////////////
        // Token Productions
        //////////////////////////////////////////////////////////////////////////

        private HaystackToken ReadId()
        {
            StringBuilder s = new StringBuilder();
            while (isIdPart(_currentChar))
            {
                s.Append((char)_currentChar);
                consume();
            }
            _currentValue = s.ToString();
            return HaystackToken.id;
        }

        private static bool isIdStart(int cur)
        {
            if (cur < 0) return false;
            if ((char)cur == '^') return true;
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

        private HaystackToken ReadNumber()
        {
            string strDecNumSep = _numberFormat.NumberDecimalSeparator;
            char cNumSep = '.';
            if (strDecNumSep.Length == 1)
                cNumSep = strDecNumSep[0];
            bool bHex = false;
            if (_currentChar < 0) err("unexpected eof in num");
            if (!(_currentChar < 0 || _peekChar < 0))
            {
                // hex number (no unit allowed)
                bHex = (char)_currentChar == '0' && (char)_peekChar == 'x';
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
                    if (HaystackTokenizer.isHex(_currentChar))
                    {
                        sb01.Append((char)_currentChar);
                        consume();
                    }
                    else if (_currentChar == '_')
                        consume();
                    else
                        bExit = true;
                }
                _currentValue = new HaystackNumber(long.Parse(sb01.ToString(), NumberStyles.AllowHexSpecifier));
                return HaystackToken.num;
            }
            // consume all things that might be part of this number token
            var builder = new StringBuilder().Append((char)_currentChar);
            consume();
            int colons = 0;
            int dashes = 0;
            int unitIndex = 0;
            bool exp = false;
            bExit = false;
            while (!bExit)
            {
                if (_currentChar >= 0) // Check for eof
                {
                    if (!char.IsDigit((char)_currentChar))
                    {
                        if (exp && (_currentChar == '+' || _currentChar == '-')) { }
                        else if (_currentChar == '-') { ++dashes; }
                        else if ((_peekChar >= 0) && (_currentChar == ':') && (char.IsDigit((char)_peekChar)))
                        {
                            ++colons;
                        }
                        else if ((exp || colons >= 1) && _currentChar == '+') { }
                        else if (_currentChar == cNumSep)
                        {
                            if (_peekChar >= 0)
                            {
                                if (!char.IsDigit((char)_peekChar)) break;
                            }
                        }
                        else if ((_peekChar >= 0) && (((char)_currentChar == 'e' || (char)_currentChar == 'E') && ((char)_peekChar == '-' || (char)_peekChar == '+' || char.IsDigit((char)_peekChar))))
                        {
                            exp = true;
                        }
                        else if (char.IsLetter((char)_currentChar) || _currentChar == '%' || _currentChar == '$' || _currentChar == '/' || _currentChar > 128) { if (unitIndex == 0) unitIndex = builder.Length; }
                        else if ((_peekChar >= 0) && (_currentChar == '_'))
                        {
                            if (unitIndex == 0 && char.IsDigit((char)_peekChar))
                            {
                                consume();
                                continue;
                            }
                            else
                            {
                                if (unitIndex == 0)
                                    unitIndex = builder.Length;
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
                    builder.Append((char)_currentChar);
                    consume();
                }
            }

            if (dashes == 2 && colons == 0) return date(builder.ToString());
            if (dashes == 0 && colons >= 1) return time(builder, colons == 1);
            if (dashes >= 2) return dateTime(builder);
            return number(builder.ToString(), unitIndex);
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
            DateTime dtParsed;
            if (!DateTime.TryParseExact(s, "yyyy'-'MM'-'dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtParsed))
            {
                throw new FormatException("Invalid date string: " + s);
            }
            _currentValue = new HaystackDate(dtParsed.Year, dtParsed.Month, dtParsed.Day);
            return HaystackToken.date;
        }

        // we don't require hour to be two digits and we don't require seconds 
        private HaystackToken time(StringBuilder s, bool addSeconds)
        {
            if (s[1] == ':')
            {
                s.Insert(0, '0');
            }
            if (addSeconds)
            {
                s.Append(":00");
            }

            string strToConv = s.ToString();
            DateTime dtParsed = DateTime.Now;
            string strFormat = "";
            if (strToConv.Contains("."))
            {
                strFormat = "HH:mm:ss.fff";
                // Unit tests show that the fff can't be more than 3 chars
                int iDotPos = strToConv.IndexOf(".");
                if ((strToConv.Length - iDotPos > 3) && (strToConv.Length > 12))
                    strToConv = strToConv.Substring(0, 12);
                else if ((strToConv.Length - iDotPos < 4) && (strToConv.Length < 12))
                {
                    // HH:mm:ss.ff
                    int iAddZeros = 3 - (strToConv.Length - iDotPos - 1);
                    for (int i = 0; i < iAddZeros; i++)
                        strToConv += '0';
                }
            }
            else
            {
                strFormat = "HH:mm:ss";
            }
            if (!DateTime.TryParseExact(strToConv, strFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtParsed))
            {
                throw new FormatException("Invalid time string: " + s);
            }
            _currentValue = new HaystackTime(dtParsed.TimeOfDay);
            return HaystackToken.time;
        }

        private HaystackToken dateTime(StringBuilder s)
        {
            bool bFlag1 = true;
            // xxx timezone
            if ((_currentChar < 0 || _peekChar < 0) || ((char)_currentChar != ' ' || !char.IsUpper((char)_peekChar)))
            {
                if (s[s.Length - 1] == 'Z') s.Append(" UTC");
                else err("Expecting timezone");
                bFlag1 = false;
            }
            if (bFlag1)
            {
                consume();
                s.Append(' ');
                while (isIdPart(_currentChar)) { s.Append((char)_currentChar); consume(); }

                // handle GMT+xx or GMT-xx
                if ((_currentChar == '+' || _currentChar == '-') && s.ToString().EndsWith("GMT"))
                {
                    s.Append((char)_currentChar); consume();
                    while (isDigit(_currentChar)) { s.Append((char)_currentChar); consume(); }
                }
            }

            // Tested 17.06.2018 with 2018-06-17T13:00:00.123+10:00 Melbourne
            string sCurrent = s.ToString();
            bool bUTC = false;
            bool bNoZoneOffset = false;
            string strDateTimeOnly = "";
            string strDateTimeOffsetFormatSpecifier = "";
            string strHTimeZone = "";
            int iPosOfLastSecondChar, iPosOfSpaceBeforeTZID = 0;

            DateTimeOffset dto;
            TimeSpan tsOffset = new TimeSpan();
            if (sCurrent.Contains('W'))
            {
                throw new FormatException("Invalid DateTime format for string " + s + " ISO8601 W specifier not supported by this toolkit");
            }
            if (sCurrent.Trim().Contains("Z UTC"))
            {
                bUTC = true;
                int iPos = sCurrent.IndexOf('Z');
                iPosOfLastSecondChar = iPos - 1;
                strDateTimeOnly = sCurrent.Substring(0, iPos);
            }
            else if (sCurrent.Trim().Contains("Z"))
            {
                bUTC = true;
                int iPos = sCurrent.IndexOf('Z');
                iPosOfLastSecondChar = iPos - 1;
                strDateTimeOnly = sCurrent.Substring(0, iPos);
            }
            if (!sCurrent.Contains('T'))
            {
                // Only possible in ISO 8601 with just a date - this is not an allowed case for Haystack DateTime
                throw new FormatException("Invalid DateTime format for string " + s + " missing ISO8601 T specifier");
            }
            // if it is offset with name then it must have a + or - sign for the offset after the T 
            int iPosOfT = sCurrent.IndexOf('T');
            if (iPosOfT + 1 >= sCurrent.Length)
            {
                // Nothing after 'T' this is not legal
                throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier");
            }

            // Stip of the timezone by finding the space
            iPosOfSpaceBeforeTZID = sCurrent.IndexOf(' ', iPosOfT);
            int iEndLen = iPosOfSpaceBeforeTZID - (iPosOfT + 1);
            string sEnd = sCurrent.Substring(iPosOfT + 1, iEndLen);
            if (!bUTC)
            {
                if ((sEnd.Trim().Contains('+')) || (sEnd.Trim().Contains('-')))
                {
                    bool bPositive = false;
                    int iPosSign = 0;
                    // In ISO 8601 this is either a +/-hh:mm or +/-hhmm or +/-hh
                    // See how many characters there is till space - that is the offset specifier
                    if (sEnd.Trim().Contains('+'))
                    {
                        bPositive = true;
                        iPosSign = sCurrent.IndexOf('+', iPosOfT);
                    }
                    else
                        iPosSign = sCurrent.IndexOf('-', iPosOfT);
                    iPosOfLastSecondChar = iPosSign - 1;
                    strDateTimeOnly = sCurrent.Substring(0, iPosSign);
                    // Find the next space - requires it contains a Haystack Zone specifier if not UTC
                    int iPosSpace = sCurrent.Trim().IndexOf(' ', iPosSign);
                    if ((iPosSpace < iPosSign) || (iPosSpace < 0))
                    {
                        throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier");
                    }
                    // What is the number of characters between the sign and the space - 5 4 or 2
                    iPosOfSpaceBeforeTZID = iPosSpace;
                    int iNumOffsetChars = iPosSpace - iPosSign - 1;
                    string strOffset = sCurrent.Substring(iPosSign + 1, iNumOffsetChars);
                    if (iNumOffsetChars == 5)
                    {
                        // Assume +/-hh:mm
                        string[] strHM = strOffset.Split(':');
                        if (strHM.Length != 2)
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }

                        int iHour;
                        if (!int.TryParse(strHM[0], out iHour))
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if (iHour < 0)
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }

                        int iMinute;
                        if (!int.TryParse(strHM[1], out iMinute))
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if ((iMinute < 0) || (iMinute > 60))
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if (!bPositive)
                        {
                            // Problem: if minute is non-zero we will get undesired result e.g. -10:10 will return -9:50 therefore both must be negative
                            iMinute = iMinute * -1;
                            iHour = iHour * -1;
                        }
                        tsOffset = new TimeSpan(iHour, iMinute, 0);
                    }
                    else if (iNumOffsetChars == 4)
                    {
                        int iHour;
                        // Assume hhmm
                        if (!int.TryParse(strOffset.Substring(0, 2), out iHour))
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if (iHour < 0)
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }

                        int iMinute;
                        if (!int.TryParse(strOffset.Substring(2, 2), out iMinute))
                        {
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if ((iMinute < 0) || (iMinute > 60))
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if (!bPositive)
                        {
                            // Problem: if minute is non-zero we will get undesired result e.g. -10:10 will return -9:50 therefore both must be negative
                            iMinute = iMinute * -1;
                            iHour = iHour * -1;
                        }
                        tsOffset = new TimeSpan(iHour, iMinute, 0);
                    }
                    else if (iNumOffsetChars == 2)
                    {
                        // Assume hh
                        int iHour;
                        // Assume hhmm
                        if (!int.TryParse(strOffset, out iHour))
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if (iHour < 0)
                        {
                            // Invalid offset
                            throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                        }
                        if (!bPositive)
                        {
                            iHour = iHour * -1;
                        }
                        tsOffset = new TimeSpan(iHour, 0, 0);
                    }
                    else
                    {
                        // Invalid offset
                        throw new FormatException("Invalid DateTime format for string " + s + " missing suitable length string after ISO8601 T specifier for offset");
                    }

                }
                else
                {
                    // Must not contain a Zone offset string
                    bNoZoneOffset = true;
                    iPosOfSpaceBeforeTZID = sCurrent.IndexOf(' ', iPosOfT);
                    strDateTimeOnly = sCurrent.Substring(0, iPosOfSpaceBeforeTZID);
                }
            }
            // Get the Haystack time zone identifier and create the HTimeZone object.
            if ((iPosOfSpaceBeforeTZID < sCurrent.Length) && (sCurrent.Length - iPosOfSpaceBeforeTZID > 2) && (!bUTC))
                strHTimeZone = sCurrent.Substring(iPosOfSpaceBeforeTZID + 1);
            // Check for the 'T' for the Formats that include that
            // Check for milliseconds
            if (strDateTimeOnly.Trim().Contains("."))
            {
                // All fields with 'T'
                strDateTimeOffsetFormatSpecifier = "yyyy-MM-dd'T'HH:mm:ss.FFF";
            }
            else
            {
                // no milliseconds
                // DateTimeOffset will adopt 0 milliseconds
                strDateTimeOffsetFormatSpecifier = "yyyy-MM-dd'T'HH:mm:ss";
            }
            try
            {
                DateTime dt = DateTime.ParseExact(strDateTimeOnly, strDateTimeOffsetFormatSpecifier, CultureInfo.InvariantCulture);
                if (!bNoZoneOffset)
                    dto = new DateTimeOffset(dt, tsOffset);
                else if (bUTC)
                {
                    DateTime dtutc = System.DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    dto = new DateTimeOffset(dtutc);
                }
                else
                    dto = new DateTimeOffset(dt);
            }
            catch (Exception)
            {
                throw new FormatException("Invalid DateTime format for string " + s);
            }
            HaystackTimeZone htz;
            if (bUTC)
            {
                htz = HaystackTimeZone.UTC;
            }
            else
            {
                try
                {
                    htz = new HaystackTimeZone(strHTimeZone);
                }
                catch (Exception genexcep)
                {
                    throw new FormatException("Invalid DateTime format for string " + s + " for Timezone [" + genexcep.Message + "]");
                }
            }
            _currentValue = new HaystackDateTime(dto, htz);
            return HaystackToken.dateTime;
        }

        private HaystackToken number(string s, int unitIndex)
        {
            try
            {
                if (unitIndex == 0)
                {
                    _currentValue = new HaystackNumber(double.Parse(s, _numberFormat));
                }
                else
                {
                    string doubleStr = s.Substring(0, unitIndex);
                    string unitStr = s.Substring(unitIndex);
                    _currentValue = new HaystackNumber(double.Parse(doubleStr, _numberFormat), unitStr);
                }
            }
            catch (Exception)
            {
                err("[HaystackTokenizer::number]Invalid Number literal: " + s);
            }
            return HaystackToken.num;
        }

        private HaystackToken ReadString()
        {
            consume('"');
            StringBuilder s = new StringBuilder();
            while (true)
            {
                if (_currentChar == _endOfFile) err("Unexpected end of str");
                if ((char)_currentChar == '"') { consume('"'); break; }
                if ((char)_currentChar == '\\') { s.Append(escape()); continue; }
                s.Append((char)_currentChar);
                consume();
            }
            _currentValue = new HaystackString(s.ToString());
            return HaystackToken.str;
        }

        private HaystackToken ReadReference()
        {
            if (_currentChar < 0) err("Unexpected eof in refh");
            consume('@');
            StringBuilder s = new StringBuilder();
            while (true)
            {
                if (HaystackValidator.IsReferenceIdChar((char)_currentChar))
                {
                    s.Append((char)_currentChar);
                    consume();
                }
                else
                {
                    break;
                }
            }
            _currentValue = new HaystackReference(s.ToString(), null);
            return HaystackToken.@ref;
        }

        private HaystackToken ReadUri()
        {
            if (_currentChar < 0) err("Unexpected end of uri");
            consume('`');
            StringBuilder s = new StringBuilder();
            while (true)
            {
                if ((_currentChar < (int)char.MinValue) || (_currentChar > (int)char.MaxValue))
                    err("Unexpected character in uri at a value of " + _currentChar.ToString());
                if ((char)_currentChar == '`')
                {
                    consume('`');
                    break;
                }
                if (_currentChar == _endOfFile || (char)_currentChar == '\n') err("Unexpected end of uri");
                if ((char)_currentChar == '\\')
                {
                    switch ((char)_peekChar)
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
                            s.Append((char)_currentChar);
                            s.Append((char)_peekChar);
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
                    s.Append((char)(_currentChar));
                    consume();
                }
            }
            _currentValue = new HaystackUri(s.ToString());
            return HaystackToken.uri;
        }

        private char escape()
        {
            if (_currentChar < 0) err("unexpected eof in escape");
            consume('\\');
            switch ((char)_currentChar)
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
            if ((char)_currentChar == 'u')
            {
                consume('u');
                esc.Append((char)_currentChar); consume();
                esc.Append((char)_currentChar); consume();
                esc.Append((char)_currentChar); consume();
                esc.Append((char)_currentChar); consume();
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
            err("Invalid escape sequence: " + (char)_currentChar);
            return (char)0x00; // this code will never execute because err throws an exception
        }

        private HaystackToken symbol()
        {
            if (_currentChar == _endOfFile) return HaystackToken.eof;
            int c = _currentChar;
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
                    if ((char)_currentChar == '<') { consume('<'); return HaystackToken.lt2; }
                    if ((char)_currentChar == '=') { consume('='); return HaystackToken.ltEq; }
                    return HaystackToken.lt;
                case '>':
                    if ((char)_currentChar == '>') { consume('>'); return HaystackToken.gt2; }
                    if ((char)_currentChar == '=') { consume('='); return HaystackToken.gtEq; }
                    return HaystackToken.gt;
                case '-':
                    if ((char)_currentChar == '>') { consume('>'); return HaystackToken.arrow; }
                    return HaystackToken.minus;
                case '=':
                    if ((char)_currentChar == '=') { consume('='); return HaystackToken.eq; }
                    return HaystackToken.assign;
                case '!':
                    if ((char)_currentChar == '=') { consume('='); return HaystackToken.notEq; }
                    return HaystackToken.bang;
                case '/':
                    return HaystackToken.slash;
            }
            if (_currentChar == _endOfFile) return HaystackToken.eof;
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
                if (_currentChar == '\n' || _currentChar == _endOfFile)
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
                if ((char)_currentChar == '*' && (char)_peekChar == '/') { consume('*'); consume('/'); depth--; if (depth <= 0) break; }
                if ((char)_currentChar == '/' && (char)_peekChar == '*') { consume('/'); consume('*'); depth++; continue; }
                if ((char)_currentChar == '\n') ++_currentLineNumber;
                if (_currentChar == _endOfFile)
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
            throw new FormatException(msg + " [line " + _currentLineNumber + "]");
        }

        //////////////////////////////////////////////////////////////////////////
        // Char
        //////////////////////////////////////////////////////////////////////////

        private void consume(int expected)
        {
            if (_currentChar != expected) err("Expected " + (char)expected);
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
                _currentChar = _peekChar;
                _peekChar = _sourceReader.Read();
                if (_peekChar < 0) // End of stream
                {
                    _peekChar = _endOfFile;
                }
            }
            catch (IOException)
            {
                _peekChar = _endOfFile;
            }
        }
    }
}