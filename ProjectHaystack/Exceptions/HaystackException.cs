using System;

namespace ProjectHaystack
{
    /// <summary>
    /// Exception thrown when a grid is returned with the err marker tag
    /// indicating a server side error.
    /// </summary>
    public class HaystackException : Exception
    {
        public HaystackGrid Grid { get; private set; }

        public HaystackException(HaystackGrid grid)
            : base(ReadStringOrNull(grid, "dis") ?? "server side error")
        {
            Grid = grid;
        }

        public string ReadMessage => ReadStringOrNull(Grid, "dis");

        /// <summary>
        /// Read server side stack trace.
        /// </summary>
        /// <returns>Stack trace or null.</returns>
        public string ReadTrace() => ReadStringOrNull(Grid, "errTrace");


        private static string ReadStringOrNull(HaystackGrid grid, string tagName)
        {
            if (!grid.Meta.ContainsKey(tagName))
            {
                return null;
            }
            var dis = grid.Meta[tagName];
            return dis is HaystackString str ? str.Value : null;
        }
    }
}