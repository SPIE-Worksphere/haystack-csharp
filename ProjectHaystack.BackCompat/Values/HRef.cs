using System;
using ProjectHaystack.io;
using ProjectHaystack.Validation;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackReference")]
    public class HRef : HVal
    {
        public HRef(HaystackReference source)
        {
            Source = source;
        }
        public HaystackReference Source { get; }
        public string val => Source.Value;
        public bool disSet => Source.Display != null;
        public static HRef make(string val, string dis) => M.Map(new HaystackReference(val, dis));
        public static HRef make(string val) => M.Map(new HaystackReference(val));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HRef str && Source.Equals(M.Map(str));
        public string display() => Source.Display;
        public string toCode() => "@" + Source.Value;
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string ToString() => Source.Value;
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public static bool isId(string id) => HaystackValidator.IsReferenceId(id);
        public static bool isIdChar(int ch) => HaystackValidator.IsReferenceIdChar((char)ch);
        public static HRef nullRef = M.Map(new HaystackReference(null));
    }
}