using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack history item containing a timestamp and a value.
    /// </summary>
    public class HaystackHistoryItem : HaystackValue
    {
        public HaystackHistoryItem(HaystackDateTime timeStamp, HaystackValue Value)
        {
            TimeStamp = timeStamp ?? throw new ArgumentNullException(nameof(timeStamp));
            this.Value = Value ?? throw new ArgumentNullException(nameof(Value));
        }

        public HaystackHistoryItem(HaystackDictionary dictionary)
        {
            if (dictionary is null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            TimeStamp = (HaystackDateTime)dictionary["ts"];
            Value = dictionary.ContainsKey("val") ? dictionary["val"] : null;
        }

        public HaystackDateTime TimeStamp { get; }

        public HaystackValue Value { get; }

        /// <summary>
        /// Read a grid of "ts" and "val" data.
        /// </summary>
        /// <param name="grid">Grid to read.</param>
        /// <returns>List of history items.</returns>
        public static IEnumerable<HaystackHistoryItem> ReadGrid(HaystackGrid grid)
        {
            var ts = grid.Column("ts");
            var val = grid.Column("val");
            return grid.Rows
                .Select(row => new HaystackHistoryItem(row));
        }

        public override int GetHashCode() => TimeStamp.GetHashCode();

        public override bool Equals(object other)
        {
            return other != null
                && other is HaystackHistoryItem historyItem
                && historyItem.TimeStamp.Equals(TimeStamp)
                && historyItem.Value.Equals(Value);
        }
    }
}