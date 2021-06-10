using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack not available (NA) marker value.
    /// </summary>
    public class HaystackNotAvailable : HaystackValue
    {
        public static HaystackNotAvailable Instance = new HaystackNotAvailable();

        public override int GetHashCode() => 0;

        public override bool Equals(object other) => other != null && other is HaystackNotAvailable;

        public override string ToString()
        {
            throw new NotImplementedException("ToString is not implemented to prevent inconsistent results.");
        }
    }
}