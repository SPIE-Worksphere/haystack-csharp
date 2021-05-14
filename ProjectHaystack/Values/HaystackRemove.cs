namespace ProjectHaystack
{
    /// <summary>
    /// Haystack remove marker value.
    /// </summary>
    public class HaystackRemove : HaystackValue
    {
        public static HaystackRemove Instance = new HaystackRemove();

        public override int GetHashCode() { return 0; }

        public override bool Equals(object other) => other != null && other is HaystackRemove;
    }
}