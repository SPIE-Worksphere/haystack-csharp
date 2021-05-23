using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackBinary")]
    public class HBin : HVal
    {
        public HBin(HaystackBinary source)
        {
            Source = source;
        }
        public HaystackBinary Source { get; }
        public string mime => Source.Mime;
        public static HBin make(string strMime) => M.Map(new HaystackBinary(strMime));
        public override int GetHashCode() => mime.GetHashCode();
        public override bool Equals(object that) => that != null && that is HBin dict && Source.Equals(M.Map(dict));
        public override string toZinc() => ZincWriter.ToZinc(Source);
        public override string toJson() => HaysonWriter.ToHayson(Source);
    }
}