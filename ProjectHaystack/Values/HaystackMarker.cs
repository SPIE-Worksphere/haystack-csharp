namespace ProjectHaystack
{
    /// <summary>
    /// Haystack marker value.
    /// </summary>
    public class HaystackMarker : HaystackValue
    {
        public static readonly HaystackMarker Instance = new HaystackMarker();

        public override int GetHashCode() { return 0; }

        public override bool Equals(object other) => other != null && other is HaystackMarker;
    }
}