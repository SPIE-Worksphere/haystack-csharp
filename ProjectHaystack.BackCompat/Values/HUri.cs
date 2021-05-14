using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackUri")]
    public class HUri : HVal
    {
        public HUri(HaystackUri source)
        {
            Source = source;
        }
        public HaystackUri Source { get; }
        public string UriVal => Source.Value;
        public static HUri make(string val) => M.Map(new HaystackUri(val));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HUri uri && Source.Equals(M.Map(uri));
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
    }
}