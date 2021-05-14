using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackGrid")]
    public class HGrid : HVal, IEnumerable<HRow>
    {
        public HGrid(HaystackGrid source)
        {
            Source = source;
        }
        public HaystackGrid Source { get; }

        internal HGrid(HDict meta, List<HCol> cols, List<List<HVal>> rowLists)
        {
            Source = new HaystackGrid(cols.Select(M.Map).ToList(), rowLists.Select(l => l.Select(M.Map).ToArray()).ToList(), M.Map(meta));
        }
        public static HGrid InstanceEmpty { get; } = M.Map(new HaystackGrid());

        public HDict meta => M.Map(Source.Meta);
        public bool isErr() => Source.IsError();
        public bool isEmpty() => Source.IsEmpty();
        public int numRows => Source.RowCount;
        public HRow row(int row) => M.Map(Source.Row(row));
        public int numCols => Source.ColumnCount;
        public HCol col(int index) => M.Map(Source.Column(index));
        public HCol col(string name) => M.Map(Source.Column(name));
        public HCol col(string name, bool bchecked) => M.Checked(() => M.Map(Source.Column(name)), bchecked);
        public override string toZinc() => ZincWriter.ToZinc(Source);
        public override string toJson() => HaysonWriter.ToHayson(Source);
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HGrid grid && Source.Equals(M.Map(grid));
        public void dump()
        {
            Debug.WriteLine(ZincWriter.ToZinc(Source));
            Debug.Flush();
        }
        public void dumpToConsole()
        {
            Console.WriteLine(ZincWriter.ToZinc(Source));
        }
        public string dumpAsString() => ZincWriter.ToZinc(Source);
        public IEnumerable<HCol> Cols => Source.Columns.Select(M.Map);
        public IEnumerable<HRow> Rows => Source.Rows.Select(M.Map);
        public IEnumerator<HRow> GetEnumerator() => Source.Select(M.Map).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}