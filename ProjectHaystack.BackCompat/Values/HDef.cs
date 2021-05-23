using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackDefinition")]
    public class HDef : HVal
    {
        public HDef(HaystackDefinition source)
        {
            Source = source;
        }
        public HaystackDefinition Source { get; }
        public static HDef make(string val) => M.Map(ZincReader.ReadValue<HaystackDefinition>(val));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HDef date && Source.Equals(M.Map(date));
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
    }
}