using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackBoolean")]
    public class HBool : HVal
    {
        public static readonly HBool TRUE = new HBool(true);
        public static readonly HBool FALSE = new HBool(false);
        public HBool(HaystackBoolean source)
        {
            Source = source;
        }
        private HBool(bool val)
        {
            Source = new HaystackBoolean(val);
        }
        public HaystackBoolean Source { get; }
        public static HBool make(bool bVal) => new HBool(new HaystackBoolean(bVal));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HBool @bool && Source.Equals(M.Map(@bool));
        public bool val => Source.Value;
        public override string ToString() => Source.Value.ToString();
        public override string toJson() => HaysonWriter.ToHayson(Source);
        public override string toZinc() => ZincWriter.ToZinc(Source);
    }
}