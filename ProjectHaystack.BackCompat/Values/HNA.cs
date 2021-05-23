using System;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackNotAvailable")]
    public class HNA : HVal
    {
        public HNA(HaystackNotAvailable source)
        {
            Source = source;
        }
        public HaystackNotAvailable Source { get; }
        public static HNA VAL = M.Map(new HaystackNotAvailable());

        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HNA;
        public override string ToString() { return "na"; }
        public override string toJson() { return "z:"; }
        public override string toZinc() { return "NA"; }
    }
}