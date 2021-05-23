namespace ProjectHaystack
{
    /// <summary>
    /// Haystack date and time range.
    /// Inclusive start, exclusive end.
    /// </summary>
    public class HaystackDateTimeRange
    {
        public HaystackDateTimeRange(HaystackDateTime start, HaystackDateTime end)
        {
            Start = start;
            End = end;
        }

        public HaystackDateTime Start { get; }
        public HaystackDateTime End { get; }
    }
}