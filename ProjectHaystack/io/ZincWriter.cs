using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ProjectHaystack.io
{
    /// <summary>
    /// Writes zinc formatted values.
    /// </summary>
    public class ZincWriter : IDisposable
    {
        private static NumberFormatInfo _numberFormat = CultureInfo.InvariantCulture.NumberFormat;

        private int _version = 3;
        private StreamWriter _targetWriter;
        private bool _isInGrid = false;

        public ZincWriter(StreamWriter targetWriter)
        {
            _targetWriter = targetWriter;
        }

        public static string ToZinc(HaystackValue val)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            using (var writer = new ZincWriter(streamWriter))
            {
                writer.WriteValue(val);
                streamWriter.Flush();
                stream.Position = 0;
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public ZincWriter WriteValue(HaystackValue value)
        {
            if (value == null)
            {
                WriteValue('N');
            }
            else if (value is HaystackBinary bin)
            {
                WriteValue("Bin(\"").WriteValue(bin.Mime).WriteValue("\")");
            }
            else if (value is HaystackBoolean boolean)
            {
                WriteValue(boolean.Value ? "T" : "F");
            }
            else if (value is HaystackCoordinate coordinate)
            {
                WriteValue($"C({coordinate.Latitude.ToString(_numberFormat)},{coordinate.Longitude.ToString(_numberFormat)})");
            }
            else if (value is HaystackDate date)
            {
                WriteValue(date.Value.ToString("yyyy-MM-dd"));
            }
            else if (value is HaystackDateTime dateTime)
            {
                var val = dateTime.Value.ToString("yyyy-MM-dd'T'HH:mm:ss.FFF");
                if (dateTime.TimeZone.Name == "UTC")
                {
                    val += "Z UTC";
                }
                else
                {
                    val += dateTime.Value.Offset.Hours >= 0 ? "+" : "-";
                    val += dateTime.Value.Offset.ToString(@"hh\:mm");
                    val += " ";
                    val += dateTime.TimeZone.Name;
                }
                WriteValue(val);
            }
            else if (value is HaystackDefinition definition)
            {
                WriteValue(definition.Value);
            }
            else if (value is HaystackDictionary dictionary)
            {
                WriteDictionary(dictionary, true);
            }
            else if (value is HaystackGrid)
            {
                HaystackGrid grid = (HaystackGrid)value;
                if (_isInGrid)
                {
                    WriteNestedGrid(grid);
                }
                else
                {
                    _isInGrid = true;
                    WriteGrid(grid);
                    _isInGrid = false;
                }
            }
            else if (value is HaystackHistoryItem historyItem)
            {
                throw new NotImplementedException("Cannot write history item");
            }
            else if (value is HaystackList list)
            {
                WriteList(list);
            }
            else if (value is HaystackMarker)
            {
                WriteValue("M");
            }
            else if (value is HaystackNotAvailable)
            {
                WriteValue("NA");
            }
            else if (value is HaystackNumber number)
            {
                if (number.Value == double.PositiveInfinity)
                {
                    WriteValue("INF");
                }
                else if (number.Value == double.NegativeInfinity)
                {
                    WriteValue("-INF");
                }
                else if (double.IsNaN(number.Value))
                {
                    WriteValue("NaN");
                }
                else
                {
                    double abs = Math.Abs(number.Value);
                    WriteValue(number.Value.ToString("#0.####", _numberFormat));
                    if (number.Unit != null)
                    {
                        WriteValue(number.Unit);
                    }
                }
            }
            else if (value is HaystackReference reference)
            {
                WriteValue($"@{reference.Value}");
                if (reference.Display != null)
                {
                    WriteValue(" ");
                    WriteValue(new HaystackString(reference.Display));
                }
            }
            else if (value is HaystackRemove)
            {
                WriteValue("R");
            }
            else if (value is HaystackString str)
            {
                var escaped = str.Value
                    .SelectMany(chr =>
                    {
                        switch (chr)
                        {
                            case ('\n'): return @"\n".ToCharArray();
                            case ('\r'): return @"\r".ToCharArray();
                            case ('\t'): return @"\t".ToCharArray();
                            case ('"'): return @"\""".ToCharArray();
                            case ('\\'): return @"\\".ToCharArray();
                            default:
                                if (chr < ' ')
                                {
                                    return (@"\u" + Convert.ToByte(chr).ToString("x4")).ToCharArray();
                                }
                                return new[] { chr };
                        }
                    })
                    .ToArray();
                WriteValue($@"""{new string(escaped)}""");
            }
            else if (value is HaystackTime time)
            {
                if (time.Value.Milliseconds == 0)
                {
                    WriteValue(time.Value.ToString("hh\\:mm\\:ss"));
                }
                else
                {
                    WriteValue(time.Value.ToString("hh\\:mm\\:ss\\.fff"));
                }
            }
            else if (value is HaystackUri uri)
            {
                var escaped = uri.Value
                    .SelectMany(chr =>
                    {
                        if (chr == '`')
                        {
                            return @"\`".ToCharArray();
                        }
                        if (chr < ' ')
                        {
                            throw new ArgumentException($"Invalid URI '{uri.Value}', char='{chr}'", "uri");
                        }
                        return new[] { chr };
                    })
                    .ToArray();
                WriteValue($@"`{new string(escaped)}`");
            }
            else if (value is HaystackXString xstr)
            {
                WriteValue(xstr.Type).WriteValue("(\"").WriteValue(xstr.Value).WriteValue("\")");
            }
            else
            {
                throw new InvalidOperationException($"Cannot write scalar value of type {value.GetType().Name}");
            }
            return this;
        }

        private void WriteNestedGrid(HaystackGrid grid)
        {
            WriteValue("<<").WriteNewline();
            WriteGrid(grid);
            WriteValue(">>");
        }

        private void WriteList(HaystackList list)
        {
            WriteValue('[');
            bool isFirst = true;
            foreach (var item in list)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    WriteValue(',');
                }
                WriteValue(item);
            }
            WriteValue(']');
        }

        public void WriteGrid(HaystackGrid grid)
        {
            // meta
            WriteValue("ver:\"").WriteValue(_version).WriteValue(".0\"").WriteMeta(grid.Meta).WriteNewline();

            // cols
            if (grid.ColumnCount == 0)
            {
                // technically this shoudl be illegal, but
                // for robustness handle it here
                throw new ArgumentException("Grid has no cols", "grid");
            }
            else
            {
                for (int i = 0; i < grid.ColumnCount; ++i)
                {
                    if (i > 0) WriteValue(',');
                    WriteColumn(grid.Column(i));
                }
            }
            WriteNewline();

            // rows
            for (int i = 0; i < grid.RowCount; ++i)
            {
                WriteRow(grid, grid.Row(i));
                WriteNewline();
            }
        }

        private ZincWriter WriteMeta(HaystackDictionary meta)
        {
            if (meta.IsEmpty()) return this;
            WriteValue(' ');
            return WriteDictionary(meta);
        }

        private ZincWriter WriteDictionary(HaystackDictionary dict, bool addBraces = false)
        {
            if (addBraces)
            {
                WriteValue('{');
            }

            bool bFirst = true;
            foreach (var kv in dict)
            {
                var name = kv.Key;
                var value = kv.Value;
                if (name != null)
                {
                    if (!bFirst) WriteValue(' ');
                    WriteValue(name);
                    if (value is not HaystackMarker)
                    {
                        WriteValue(':').WriteValue(value);
                    }
                    bFirst = false;
                }
            }

            if (addBraces)
            {
                WriteValue('}');
            }
            return this;
        }

        private void WriteColumn(HaystackColumn column)
        {
            WriteValue(column.Name).WriteMeta(column.Meta);
        }

        private void WriteRow(HaystackGrid grid, HaystackRow row)
        {
            var first = true;
            foreach (var column in grid.Columns)
            {
                var value = row.ContainsKey(column.Name) ? row[column.Name] : null;
                if (first)
                {
                    first = false;
                }
                else
                {
                    _targetWriter.Write(',');
                }
                if (value == null)
                {
                    _targetWriter.Write('N');
                }
                else
                {
                    WriteValue(value);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Member Utils - These were print - changed to write
        //////////////////////////////////////////////////////////////////////////

        private ZincWriter WriteValue(int i)
        {
            _targetWriter.Write(i);
            return this;
        }
        private ZincWriter WriteValue(char c)
        {
            _targetWriter.Write(c);
            return this;
        }
        private ZincWriter WriteValue(object obj)
        {
            _targetWriter.Write(obj);
            return this;
        }
        private ZincWriter WriteNewline()
        {
            _targetWriter.Write('\n');
            return this;
        }

        public void Dispose()
        {
            if (_targetWriter != null)
            {
                var writer = _targetWriter;
                _targetWriter = null;
                writer.Dispose();
            }
        }
    }
}