namespace ProjectHaystack
{
    /// <summary>
    /// Base class for Haystack values.
    /// </summary>
    public abstract class HaystackValue
    {
        public abstract override bool Equals(object other);
        public abstract override int GetHashCode();

        public static bool operator ==(HaystackValue left, HaystackValue right) => Equals(left, null) ? Equals(right, null) : left.Equals(right);
        public static bool operator !=(HaystackValue left, HaystackValue right) => !(left == right);
    }
}