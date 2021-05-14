using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackRemove")]
    public class HRemove : HVal
    {
        public HRemove(HaystackRemove source)
        {
            Source = source;
        }
        public static HRemove VAL = M.Map(new HaystackRemove());
        public HaystackRemove Source { get; }
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HRemove;
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
    }
}