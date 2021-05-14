using System;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackColumn")]
    public class HCol
    {
        public HCol(HaystackColumn source)
        {
            Source = source;
        }
        public HCol(int iIndex, string name, HDict meta)
        {
            Source = new HaystackColumn(iIndex, name, M.Map(meta));
        }
        public HaystackColumn Source { get; }
        public string Name => Source.Name;
        public string dis() => Source.Display;
        public HDict meta => M.Map(Source.Meta);
        public int Index => Source.Index;
        public override int GetHashCode() => Name.GetHashCode() ^ meta.GetHashCode();
        public bool hequals(object that) => Equals(that);
        public override bool Equals(object that) => that != null && that is HCol col && Source.Equals(M.Map(col));
    }
}