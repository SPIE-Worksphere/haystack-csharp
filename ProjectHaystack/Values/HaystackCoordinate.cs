using System;
using ProjectHaystack.Validation;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack world coordinate.
    /// </summary>
    public class HaystackCoordinate : HaystackValue
    {
        public HaystackCoordinate(decimal latitude, decimal longitude)
        {
            if (!HaystackValidator.IsLatitude(latitude)) throw new ArgumentException("Invalid latitude > +/- 90", "latitude");
            if (!HaystackValidator.IsLongitude(longitude)) throw new ArgumentException("Invalid longitude > +/- 180", "longitude");

            Latitude = latitude;
            Longitude = longitude;
        }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public override int GetHashCode() => Latitude.GetHashCode() ^ Longitude.GetHashCode();

        public override bool Equals(object other)
        {
            return other != null
                && other is HaystackCoordinate coordinate
                && Latitude == coordinate.Latitude
                && Longitude == coordinate.Longitude;
        }
    }
}