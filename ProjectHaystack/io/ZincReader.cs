using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectHaystack.io
{
    /// <summary>
    /// Read Haystack values from Zinc.
    /// </summary>
    public class ZincReader : IDisposable
    {
        private HaystackTokenizer _tokenizer;

        private HaystackToken _currentToken;
        private object _currentValue;
        private int _currentLineNumber;

        private HaystackToken _peekToken;
        private object _peekValue;
        private int _peekLineNumber;

        private int _version;

        public ZincReader(Stream stream)
        {
            _tokenizer = new HaystackTokenizer(new StreamReader(stream, Encoding.UTF8));
            Init();
        }

        public ZincReader(string zinc)
            : this(new MemoryStream(Encoding.UTF8.GetBytes(zinc)))
        {
        }

        public static TValue ReadValue<TValue>(string zinc)
            where TValue : HaystackValue
        {
            return new ZincReader(zinc).ReadValue<TValue>();
        }

        public static HaystackValue ReadValue(string zinc)
        {
            return new ZincReader(zinc).ReadValue<HaystackValue>();
        }

        private void Init()
        {
            // Read first token.
            Consume();
            Consume();
        }

        public void Dispose()
        {
            _tokenizer.Dispose();
        }

        /// <summary>
        /// Read a single value.
        /// </summary>
        public HaystackValue ReadValue()
        {
            HaystackValue val = null;
            bool bValisVer = false;
            if (_currentValue is string)
            {
                if (((string)_currentValue).CompareTo("ver") == 0)
                    bValisVer = true;
            }
            if (_currentToken == HaystackToken.id && bValisVer)
                val = ReadGrid();
            else
                val = ParseValue();
            // Depart from Java - 15.08.2018 after testing with Java Toolkit
            // It is possible there might be newlines or other non prihtables before eof
            // only verify it is not a token of interest
            bool bEnd = false;
            while (!bEnd)
            {
                if (TryVerify(HaystackToken.nl, false))
                    Consume();
                else
                {
                    // This will throw an exception if it is not eof
                    Verify(HaystackToken.eof);
                    bEnd = true;
                }
            }
            return val;
        }

        /// <summary>
        /// Read a single value.
        /// </summary>
        public TValue ReadValue<TValue>()
            where TValue : HaystackValue
        {
            return (TValue)ReadValue();
        }

        /// <summary>
        /// Read a list of grids separated by a blank line.
        /// </summary>
        public IEnumerable<HaystackGrid> ReadGrids()
        {
            while (_currentToken == HaystackToken.id)
                yield return ReadGrid();
        }

        /// <summary>
        /// Read a dictionary of tags.
        /// </summary>
        public HaystackDictionary ReadDictionary()
        {
            var dictionary = new HaystackDictionary();
            bool braces = _currentToken == HaystackToken.lbrace;
            if (braces) Consume(HaystackToken.lbrace);
            while (_currentToken == HaystackToken.id)
            {
                // tag name
                string id = ConsumeTagName();
                if (!char.IsLower(id[0]))
                    throw GetException("Invalid dict tag name: " + id);

                // tag value
                HaystackValue val = HaystackMarker.Instance;
                if (_currentToken == HaystackToken.colon)
                {
                    Consume(HaystackToken.colon);
                    val = ParseValue();
                }
                dictionary.Add(id, val);
            }
            if (braces) Consume(HaystackToken.rbrace);
            return dictionary;
        }

        //////////////////////////////////////////////////////////////////////////
        // Utils
        //////////////////////////////////////////////////////////////////////////

        private HaystackValue ParseValue()
        {
            // if it's an id
            if (_currentToken == HaystackToken.id)
            {
                string id = (string)_currentValue;
                Consume(HaystackToken.id);

                // check for coord or xstr
                if (_currentToken == HaystackToken.lparen)
                {
                    if (_peekToken == HaystackToken.num)
                        return ParseCoordinate(id);
                    else
                        return ParseXStr(id);
                }

                // check for keyword
                if ("T".CompareTo(id) == 0) return new HaystackBoolean(true);
                if ("F".CompareTo(id) == 0) return new HaystackBoolean(false);
                if ("N".CompareTo(id) == 0) return null;
                if ("M".CompareTo(id) == 0) return HaystackMarker.Instance;
                if ("NA".CompareTo(id) == 0) return HaystackNotAvailable.Instance;
                if ("R".CompareTo(id) == 0) return HaystackRemove.Instance;
                if ("NaN".CompareTo(id) == 0) return HaystackNumber.NaN;
                if ("INF".CompareTo(id) == 0) return HaystackNumber.POS_INF;
                if (id.StartsWith("^")) return new HaystackDefinition(id);

                throw GetException("Unexpected identifier: " + id);
            }

            // literals
            if (_currentToken.Literal) return ParseLiteral();
            bool bPeekIsINF = false;
            if (_peekValue is string)
            {
                if (((string)_peekValue).CompareTo("INF") == 0)
                    bPeekIsINF = true;
            }
            // -INF
            if (_currentToken == HaystackToken.minus && bPeekIsINF)
            {
                Consume(HaystackToken.minus);
                Consume(HaystackToken.id);
                return HaystackNumber.NEG_INF;
            }

            // nested collections
            if (_currentToken == HaystackToken.lbracket) return ParseList();
            if (_currentToken == HaystackToken.lbrace) return ReadDictionary();
            if (_currentToken == HaystackToken.lt2) return ReadGrid();

            throw GetException("Unexpected token: " + curToStr());
        }

        private HaystackCoordinate ParseCoordinate(string id)
        {

            if ("C".CompareTo(id) != 0)
                throw GetException("Expecting 'C' for coord, not " + id);
            Consume(HaystackToken.lparen);
            HaystackNumber lat = ConsumeNum();
            Consume(HaystackToken.comma);
            HaystackNumber lng = ConsumeNum();
            Consume(HaystackToken.rparen);
            return new HaystackCoordinate((decimal)lat.Value, (decimal)lng.Value);
        }

        private HaystackValue ParseXStr(string id)
        {
            if (!char.IsUpper(id[0]))
                throw GetException("Invalid XStr type: " + id);
            Consume(HaystackToken.lparen);
            if (_version < 3 && ("Bin".CompareTo(id) == 0)) return ParseBinObsolete();
            string val = ConsumeStr();
            Consume(HaystackToken.rparen);
            if (id == "Bin")
                return new HaystackBinary(val);
            return new HaystackXString(val, id);
        }

        private HaystackBinary ParseBinObsolete()
        {
            StringBuilder s = new StringBuilder();
            while (_currentToken != HaystackToken.rparen && _currentToken != HaystackToken.eof)
            {
                if (_currentValue == null) s.Append(_currentToken.Symbol);
                else s.Append(_currentValue);
                Consume();
            }
            Consume(HaystackToken.rparen);
            return new HaystackBinary(s.ToString());
        }

        private HaystackValue ParseLiteral()
        {
            object val = _currentValue;
            if (_currentToken == HaystackToken.@ref && _peekToken == HaystackToken.str)
            {
                val = new HaystackReference(((HaystackReference)val).Value, ((HaystackString)_peekValue).Value);
                Consume(HaystackToken.@ref);
            }
            Consume();
            return (HaystackValue)val;
        }

        private HaystackList ParseList()
        {
            List<HaystackValue> arr = new List<HaystackValue>();
            Consume(HaystackToken.lbracket);
            while (_currentToken != HaystackToken.rbracket && _currentToken != HaystackToken.eof)
            {
                HaystackValue val = ParseValue();
                arr.Add(val);
                if (_currentToken != HaystackToken.comma)
                    break;
                Consume(HaystackToken.comma);
            }
            Consume(HaystackToken.rbracket);
            return new HaystackList(arr);
        }


        private HaystackGrid ReadGrid()
        {
            bool nested = _currentToken == HaystackToken.lt2;
            if (nested)
            {
                Consume(HaystackToken.lt2);
                if (_currentToken == HaystackToken.nl)
                    Consume(HaystackToken.nl);
            }

            bool bValisVer = false;
            if (_currentValue is string)
            {
                if (((string)_currentValue).CompareTo("ver") == 0)
                    bValisVer = true;
            }
            // ver:"3.0"
            if (_currentToken != HaystackToken.id || !bValisVer)
                throw GetException("Expecting grid 'ver' identifier, not " + curToStr());
            Consume();
            Consume(HaystackToken.colon);
            _version = CheckVersion(ConsumeStr());

            // grid meta
            var grid = new HaystackGrid();
            if (_currentToken == HaystackToken.id)
            {
                var dict = ReadDictionary();
                foreach (var kv in dict)
                {
                    grid.AddMeta(kv.Key, kv.Value);
                }
            }
            Consume(HaystackToken.nl);

            // column definitions
            int numCols = 0;
            while (_currentToken == HaystackToken.id)
            {
                ++numCols;
                string name = ConsumeTagName();
                var colMeta = new HaystackDictionary();
                if (_currentToken == HaystackToken.id)
                    colMeta = ReadDictionary();
                grid.AddColumn(name, colMeta);
                if (_currentToken != HaystackToken.comma)
                    break;
                Consume(HaystackToken.comma);
            }
            if (numCols == 0)
                throw GetException("No columns defined");
            Consume(HaystackToken.nl);

            // grid rows
            while (true)
            {
                if (_currentToken == HaystackToken.nl) break;
                if (_currentToken == HaystackToken.eof) break;
                if (nested && _currentToken == HaystackToken.gt2) break;

                // read cells
                HaystackValue[] cells = new HaystackValue[numCols];
                for (int i = 0; i < numCols; ++i)
                {
                    if (_currentToken == HaystackToken.comma || _currentToken == HaystackToken.nl || _currentToken == HaystackToken.eof)
                        cells[i] = null;
                    else
                        cells[i] = ParseValue();
                    if (i + 1 < numCols) Consume(HaystackToken.comma);
                }
                grid.AddRow(cells);

                // newline or end
                if (nested && _currentToken == HaystackToken.gt2) break;
                if (_currentToken == HaystackToken.eof) break;
                Consume(HaystackToken.nl);
            }

            if (_currentToken == HaystackToken.nl) Consume(HaystackToken.nl);
            if (nested) Consume(HaystackToken.gt2);
            return grid;
        }

        private int CheckVersion(string s)
        {
            if ("3.0".CompareTo(s) == 0) return 3;
            if ("2.0".CompareTo(s) == 0) return 2;
            throw GetException("Unsupported version " + s);
            return -1; // This code will not be executable due to err
        }

        private string ConsumeTagName()
        {
            Verify(HaystackToken.id);
            string id = (string)_currentValue;
            if (id.Length == 0 || !char.IsLower(id[0]))
                throw GetException("Invalid dict tag name: " + id);
            Consume(HaystackToken.id);
            return id;
        }

        private HaystackNumber ConsumeNum()
        {
            Verify(HaystackToken.num);
            HaystackNumber num = (HaystackNumber)_currentValue;
            Consume(HaystackToken.num);
            return num;
        }

        private string ConsumeStr()
        {
            Verify(HaystackToken.str);
            var val = ((HaystackString)_currentValue).Value;
            Consume(HaystackToken.str);
            return val;
        }

        private void Verify(HaystackToken expected)
        {
            HaystackToken tokToCheck = _currentToken;
            if (expected == HaystackToken.eof) // eof will stop at peek
                tokToCheck = _peekToken;
            if (tokToCheck != expected)
                throw GetException("Expected " + expected + " not " + curToStr());
        }

        private bool TryVerify(HaystackToken expected, bool bErr)
        {
            HaystackToken tokToCheck = _currentToken;
            if (expected == HaystackToken.eof) // eof will stop at peek
                tokToCheck = _peekToken;
            if (tokToCheck != expected)
            {
                if (bErr)
                    throw GetException("Expected " + expected + " not " + curToStr());
                return false;
            }
            else return true;
        }
        private string curToStr()
        {
            return _currentValue != null ? _currentToken + " " + _currentValue : _currentToken.ToString();
        }

        private void Consume() => Consume(null);

        private void Consume(HaystackToken expected)
        {
            if (expected != null)
            {
                Verify(expected);
            }

            _currentToken = _peekToken;
            _currentValue = _peekValue;
            _currentLineNumber = _peekLineNumber;

            _peekToken = _tokenizer.Next();
            _peekValue = _tokenizer.Val;
            _peekLineNumber = _tokenizer.LineNumber;
        }

        private Exception GetException(string msg) => GetException(msg, null);

        private Exception GetException(string msg, Exception e)
        {
            return e != null
                ? new Exception(msg + " [line " + _currentLineNumber + "]", e)
                : new Exception(msg + " [line " + _currentLineNumber + "]");
        }
    }
}