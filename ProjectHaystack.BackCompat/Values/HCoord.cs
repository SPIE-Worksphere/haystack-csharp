using System;
using ProjectHaystack.io;
using ProjectHaystack.Validation;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackCoordinate")]
    public class HCoord : HVal
    {
        public HCoord(HaystackCoordinate source)
        {
            Source = source;
        }
        public HaystackCoordinate Source { get; }
        public static HCoord make(string s) => M.Map(ZincReader.ReadValue<HaystackCoordinate>(s));
        public static HCoord make(double dblLat, double dblLng) => M.Map(new HaystackCoordinate((decimal)dblLat, (decimal)dblLng));
        public static bool isLat(double lat) => HaystackValidator.IsLatitude((decimal)lat);
        public static bool isLng(double lng) => HaystackValidator.IsLongitude((decimal)lng);
        public double lat => (double)Source.Latitude;
        public double lng => (double)Source.Longitude;
        public int ulat => (int)(Source.Latitude * 1000000);
        public int ulng => (int)(Source.Longitude * 1000000);
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HCoord coord && Source.Equals(M.Map(coord));
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
    }
}