using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack grid consisting of columns and rows of data.
    /// </summary>
    public class HaystackGrid : HaystackValue, IEnumerable<HaystackRow>
    {
        private static readonly HaystackGrid _empty = BuildEmptyGrid();
        private List<HaystackColumn> _columns;
        private List<HaystackRow> _rows;
        private Dictionary<string, HaystackColumn> _columnsByName;

        public HaystackGrid()
        {
            Meta = new HaystackDictionary();
            _columns = new List<HaystackColumn>();
            _rows = new List<HaystackRow>();
            _columnsByName = new Dictionary<string, HaystackColumn>();
        }

        public HaystackGrid(List<HaystackColumn> columns, List<HaystackValue[]> rowData, HaystackDictionary meta = null)
        {
            var duplicates = columns
                .GroupBy(col => col.Name)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key)
                .ToArray();
            if (duplicates.Any())
            {
                throw new ArgumentException($"Detected duplicate column(s): {string.Join(", ", duplicates)}");
            }
            _columns = columns;
            _columnsByName = _columns.ToDictionary(col => col.Name);

            _rows = rowData.Select(row => new HaystackRow(this, row)).ToList();

            Meta = meta ?? new HaystackDictionary();
        }

        public HaystackGrid(HaystackDictionary[] rows, HaystackDictionary meta = null)
        {
            var columnNames = rows
                .SelectMany(row => row.Keys)
                .Distinct()
                .ToArray();
            _columns = columnNames
                .Select((name, idx) => new HaystackColumn(idx, name))
                .ToList();
            _columnsByName = _columns.ToDictionary(col => col.Name);

            _rows = rows
                .Select(row => new HaystackRow(this, columnNames.Select(col => row.ContainsKey(col) ? row[col] : null).ToArray()))
                .ToList();

            Meta = meta ?? new HaystackDictionary();
        }

        public HaystackGrid(HaystackHistoryItem[] historyItems, HaystackDictionary meta = null)
        {
            _columns = new[] { new HaystackColumn(0, "ts"), new HaystackColumn(1, "val") }.ToList();
            _rows = historyItems.Select(item => new HaystackRow(this, item.TimeStamp, item.Value)).ToList();
            Meta = meta ?? new HaystackDictionary();
        }

        public static HaystackGrid Empty => _empty;

        public HaystackDictionary Meta { get; }

        public IEnumerable<HaystackColumn> Columns => _columns.AsEnumerable();
        public int ColumnCount => _columns.Count;

        public IEnumerable<HaystackRow> Rows => _rows.AsEnumerable();
        public int RowCount => _rows.Count;

        public bool IsError() { return Meta.ContainsKey("err"); }

        public bool IsEmpty() { return RowCount == 0; }

        public HaystackRow Row(int row) { return _rows[row]; }
        public HaystackRow this[int row] { get => _rows[row]; }

        public bool HasColumn(string name) => _columnsByName.ContainsKey(name);

        public HaystackColumn Column(int index) => _columns[index];

        public HaystackColumn Column(string name) => HasColumn(name) ? _columnsByName[name] : throw new HaystackUnknownNameException(name);

        public HaystackGrid AddMeta(string key, HaystackValue value)
        {
            Meta.Add(key, value);
            return this;
        }

        public HaystackGrid AddColumn(string name, Action<HaystackColumn> configure = null)
        {
            if (_columnsByName.ContainsKey(name))
            {
                throw new ArgumentException($"Detected duplicate column: {name}");
            }
            if (_rows.Any())
            {
                throw new InvalidOperationException("Cannot add columns after rows");
            }

            var col = new HaystackColumn(_columns.Count, name, new HaystackDictionary());
            if (configure != null)
            {
                configure(col);
            }
            _columns.Add(col);
            _columnsByName[col.Name] = col;
            return this;
        }

        public HaystackGrid AddColumn(string name, HaystackDictionary metaData)
        {
            return AddColumn(name, col =>
            {
                foreach (var kv in metaData)
                {
                    col.Meta.Add(kv);
                }
            });
        }

        public HaystackGrid AddRow(params HaystackValue[] values)
        {
            _rows.Add(new HaystackRow(this, values));
            return this;
        }

        public override int GetHashCode() => Columns.Aggregate(0, (x, col) => x ^ col.GetHashCode());

        public override bool Equals(object other)
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
            if (other == null || !(other is HaystackGrid grid))
            {
                return false;
            }
#else
            if (other == null || other is not HaystackGrid grid)
            {
                return false;
            }
#endif
            if (other.Equals(this))
            {
                return true;
            }

            return Meta.Equals(grid.Meta)
                && ColumnsEquals(grid)
                && RowsEquals(grid);
        }

        private bool ColumnsEquals(HaystackGrid grid)
        {
            return ColumnCount == grid.ColumnCount
                && _columns
                    .Select((col, idx) => col.Equals(grid._columns[idx]))
                    .All(x => x);
        }

        private bool RowsEquals(HaystackGrid grid)
        {
            return RowCount == grid.RowCount
                && _rows
                    .Select((row, idx) => row.Equals(grid._rows[idx]))
                    .All(x => x);
        }

        public IEnumerator<HaystackRow> GetEnumerator() => _rows.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _rows.GetEnumerator();

        private static HaystackGrid BuildEmptyGrid()
        {
            List<HaystackColumn> col = new List<HaystackColumn>();
            col.Add(new HaystackColumn(0, "empty", new HaystackDictionary()));
            return new HaystackGrid(col, new List<HaystackValue[]>());
        }
    }
}