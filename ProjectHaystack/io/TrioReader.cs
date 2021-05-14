using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjectHaystack.io
{
    public class TrioReader : IDisposable
    {
        private TextReader _trioReader;
        // Strings can be left unquoted if they begin with any of these "safe" chars:
        // Any non-ASCII Unicode character, A-Z, a-z, underbar, dash, or space.
        private Regex _safeCharsRegex = new Regex(@"^([^\x00-\x7F]|[A-Za-z_\- ])", RegexOptions.Compiled);

        public TrioReader(string trio)
        {
            _trioReader = new StringReader(trio);
        }

        public TrioReader(Stream trioStream)
        {
            _trioReader = new StreamReader(trioStream, null, false, 1024, true);
        }

        public HaystackGrid ToGrid()
        {
            // Gather entities and unique keys.
            var entities = ReadEntities().ToArray();
            var keys = entities.SelectMany(d => d.Keys).Distinct().ToArray();

            // Build the grid.
            var grid = new HaystackGrid();
            foreach (var key in keys)
            {
                grid.AddColumn(key);
            }
            foreach (var entity in entities)
            {
                grid.AddRow(keys.Select(key => entity.ContainsKey(key) ? entity[key] : null).ToArray());
            }
            return grid;
        }

        public IEnumerable<HaystackDictionary> ReadEntities()
        {
            var entity = new Dictionary<string, HaystackValue>();

            string line = _trioReader.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                // Comments start with //
                if (line.StartsWith("//"))
                {
                    line = _trioReader.ReadLine();
                    continue;
                }
                // Lines starting with a - separate entities.
                if (line.StartsWith("-"))
                {
                    // If some values are collected, yield the entity.
                    if (entity.Count > 0)
                    {
                        yield return new HaystackDictionary(entity);
                        entity = new Dictionary<string, HaystackValue>();
                    }
                    // Move to the next line and continue.
                    line = _trioReader.ReadLine();
                    continue;
                }
                // Each line must start with a tag, optionally followed by a colon and a value, like "tag:value".
                var parts = line.Split(new[] { ':' }, 2);
                if (!string.IsNullOrEmpty(parts[0]))
                {
                    var key = parts[0];

                    if (line.EndsWith(":"))
                    {
                        // Multi-line value.
                        var textFormat =
                            parts.Length == 1 ? TextFormat.String
                            : parts[1].EndsWith("Zinc:") ? TextFormat.Zinc
                            : parts[1].EndsWith("Trio:") ? TextFormat.Trio
                            : TextFormat.String;
                        var valBuilder = new StringBuilder();
                        while ((line = _trioReader.ReadLine()) != null && line.StartsWith("  "))
                            valBuilder.AppendLine(line.Substring(2));
                        switch (textFormat)
                        {
                            case TextFormat.String:
                                entity.Add(key, new HaystackString(valBuilder.ToString().Trim()));
                                break;
                            case TextFormat.Zinc:
                                entity.Add(key, new ZincReader(valBuilder.ToString()).ReadValue());
                                break;
                            case TextFormat.Trio:
                                var entities = new TrioReader(valBuilder.ToString()).ReadEntities().ToArray();
                                var val = entities.Length == 1
                                    ? (HaystackValue)entities[0]
                                    : new HaystackList(entities);
                                entity.Add(key, val);
                                break;
                        }
                        continue;
                    }
                    else
                    {
                        if (parts.Length == 1)
                        {
                            // Marker tag.
                            entity.Add(key, new HaystackMarker());
                        }
                        else
                        {
                            var val = parts[1].Trim();
                            // Quote any "safe" strings.
                            if (_safeCharsRegex.IsMatch(val))
                            {
                                val = $@"""{val}""";
                            }
                            // Decode the value using Zinc.
                            entity.Add(key, new ZincReader(val).ReadValue());
                        }
                    }
                }

                // Move to the next line and continue.
                line = _trioReader.ReadLine();
            }
            // If some values are collected, yield the entity.
            if (entity.Count > 0)
                yield return new HaystackDictionary(entity);
        }

        public void Dispose()
        {
            _trioReader.Dispose();
        }

        private enum TextFormat
        {
            String,
            Zinc,
            Trio,
        }
    }
}