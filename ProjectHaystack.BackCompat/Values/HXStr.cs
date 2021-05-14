using System;
using ProjectHaystack.io;
using ProjectHaystack.Validation;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackXString")]
    public class HXStr : HVal
    {
        public HXStr(HaystackXString source)
        {
            Source = source;
        }

        public HaystackXString Source { get; }
        public string Type => Source.Type;
        public string Val => Source.Value;
        public static HVal decode(string type, string val)
        {
            if ("Bin".CompareTo(type) == 0) return HBin.make(val);
            return M.Map(new HaystackXString(val, type));
        }
        public static HXStr encode(object val)
        {
            return M.Map(new HaystackXString(nameof(val), val.ToString()));
        }
        private static bool isValidType(string t) => HaystackValidator.IsTypeName(t);
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HXStr str && Source.Equals(M.Map(str));
    }
}