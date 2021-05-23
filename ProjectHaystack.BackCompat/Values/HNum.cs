using System;
using ProjectHaystack.io;
using ProjectHaystack.Validation;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackNumber")]
    public class HNum : HVal
    {
        public HNum(HaystackNumber source)
        {
            Source = source;
        }
        public HaystackNumber Source { get; }
        public double doubleval => Source.Value;
        public string unit => Source.Unit;
        public static HNum make(int val) => M.Map(new HaystackNumber(val));
        public static HNum make(int val, string unit) => M.Map(new HaystackNumber(val, unit));
        public static HNum make(long val) => M.Map(new HaystackNumber(val));
        public static HNum make(long val, string unit) => M.Map(new HaystackNumber(val, unit));
        public static HNum make(double val) => M.Map(new HaystackNumber(val));
        public static HNum make(double val, string unit) => M.Map(new HaystackNumber(val, unit));
        public static HNum ZERO = M.Map(HaystackNumber.ZERO);
        public static HNum POS_INF = M.Map(HaystackNumber.POS_INF);
        public static HNum NEG_INF = M.Map(HaystackNumber.NEG_INF);
        public static HNum NaN = M.Map(HaystackNumber.NaN);
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HNum num && Source.Equals(M.Map(num));
        public int compareTo(object that) => that is HNum num ? Source.CompareTo(M.Map(num)) : 1;
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public long millis()
        {
            string u = Source.Unit;
            if (u == null) u = "null";
            if ((u.Trim() == "ms") || (u.Trim() == "millisecond")) return (long)doubleval;
            if ((u.Trim() == "s") || (u.Trim() == "sec")) return (long)(doubleval * 1000.0); // NOTE: A case was taken out of the Java here - it represented an unreachable test
            if ((u.Trim() == "min") || (u.Trim() == "minute")) return (long)(doubleval * 1000.0 * 60.0);
            if ((u.Trim() == "h") || (u.Trim() == "hr")) return (long)(doubleval * 1000.0 * 3600.0); // NOTE: A case was taken out of the Java here - it represented an unreachable test
            throw new InvalidOperationException("Invalid duration unit: " + u);
        }
        public static bool isUnitName(string strUnit) => HaystackValidator.IsUnitName(strUnit);
    }
}