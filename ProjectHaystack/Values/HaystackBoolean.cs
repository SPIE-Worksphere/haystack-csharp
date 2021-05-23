namespace ProjectHaystack
{
    /// <summary>
    /// Boolean value.
    /// </summary>
    public class HaystackBoolean : HaystackValue
    {
        private static readonly HaystackBoolean _true = new HaystackBoolean(true);
        private static readonly HaystackBoolean _false = new HaystackBoolean(false);

        public HaystackBoolean(bool value)
        {
            Value = value;
        }

        public static HaystackBoolean True => _true;
        public static HaystackBoolean False => _false;

        public bool Value { get; }

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackBoolean boolean && boolean.Value == Value;
    }
}