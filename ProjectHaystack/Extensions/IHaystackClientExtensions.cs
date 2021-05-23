using System.Threading.Tasks;

namespace ProjectHaystack.Client
{
    public static class IHaystackClientExtensions
    {
        /// <summary>
        /// Convenience method to call HisWriteNoWarnAsync with a "noWarn" marker to
        /// prevent warnings when writing out-of-order data.
        /// <param name="id">Record ID.</param>
        /// <param name="items">Time-series data.</param>
        public static Task<HaystackGrid> HisWriteNoWarnAsync(this IHaystackClient client, HaystackReference id, HaystackHistoryItem[] items)
        {
            var meta = new HaystackDictionary();
            meta.Add("noWarn", new HaystackMarker());
            return client.HisWriteAsync(id, items, meta);
        }
    }
}