using System;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackMarker")]
    public class HMarker : HVal
    {
        public HMarker(HaystackMarker source)
        {
            Source = source;
        }
        public HaystackMarker Source { get; }
        public static readonly HMarker VAL = M.Map(new HaystackMarker());

        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HMarker;
        public override string ToString() { return "marker"; }
        public override string toJson() { return "m:"; }
        public override string toZinc() { return "M"; }
    }
}