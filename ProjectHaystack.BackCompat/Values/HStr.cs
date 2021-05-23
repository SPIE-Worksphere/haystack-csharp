using System;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackString")]
    public class HStr : HVal
    {
        public HStr(HaystackString source)
        {
            Source = source;
        }
        protected HStr(string val)
        {
            Source = new HaystackString(val);
        }
        public HaystackString Source { get; }
        public static HStr InstanceEmpty => M.Map(HaystackString.Empty);
        public static HStr make(string val) => M.Map(new HaystackString(val));
        public string Value => Source.Value;

        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HStr str && Source.Equals(M.Map(str));
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public static string toCode(string val) => ZincWriter.ToZinc(new HaystackString(val));
        public static string[] customSplitWithTrim(string str, char[] cSeps, bool bEmpty)
        {
            string[] strARet;
            if (bEmpty)
                strARet = str.Split(cSeps, StringSplitOptions.RemoveEmptyEntries);
            else
                strARet = str.Split(cSeps);
            for (int i = 0; i < strARet.Length; i++)
                strARet[i] = strARet[i].Trim();
            return strARet;
        }
    }
}