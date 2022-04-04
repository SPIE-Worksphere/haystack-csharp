using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectHaystack
{
    /// <summary>
    /// Row in a Haystack grid.
    /// </summary>
    public class HaystackRow : HaystackDictionary
    {
        public HaystackRow(HaystackGrid grid, params HaystackValue[] values)
            : base(new Lazy<IDictionary<string, HaystackValue>>(() => ToDictionary(grid, values)))
        {
            Grid = grid;
        }

        public HaystackGrid Grid { get; }

        public override ICollection<HaystackValue> Values => _source.Value.Values;

        public override void Add(string key, HaystackValue value)
        {
            throw new InvalidOperationException("Cannot add values to a row as it will affect the entire grid");
        }

        public override bool Remove(string key)
        {
            throw new InvalidOperationException("Cannot remove values from a row as it will affect the entire grid");
        }

        public HaystackValue this[int index]
        {
            get => Get(Grid.Column(index).Name);
            set
            {
                var key = Grid.Column(index).Name;
                _source.Value[key] = value;
            }
        }

        public override HaystackValue this[string key]
        {
            get => Get(key);
            set
            {
                if (!_source.Value.ContainsKey(key))
                    throw new HaystackUnknownNameException("Cannot add values to a row as it will affect the entire grid");
                _source.Value[key] = value;
            }
        }

        private static IDictionary<string, HaystackValue> ToDictionary(HaystackGrid grid, HaystackValue[] values)
        {
            if (values == null || grid.ColumnCount != values.Length)
            {
                throw new ArgumentException($"Row count {values.Length} does not match col count {grid.ColumnCount}", "values");
            }

#if NETSTANDARD2_0 || NETSTANDARD2_1
            return grid.Columns
                .Select((col, idx) => new KeyValuePair<string, HaystackValue>(col.Name, values[idx]))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
#else
            return new Dictionary<string, HaystackValue>(grid.Columns
                .Select((col, idx) => new KeyValuePair<string, HaystackValue>(col.Name, values[idx])));
#endif
        }
    }
}