using System.Threading.Tasks;
using ProjectHaystack.Client;

namespace ProjectHaystack.Examples.Values
{
    /// <summary>
    /// Examples on how to use a HaystackGrid.
    /// Note that the rows and metadata inherit HaystackDictionary and can be used as such.
    /// </summary>
    public class HaystackGridExamples
    {
        private readonly IHaystackClient _haystackClient;

        public HaystackGridExamples(IHaystackClient haystackClient)
        {
            _haystackClient = haystackClient;
        }

        /// <summary>
        /// Iterate over the grid rows.
        /// </summary>
        public async Task IterateRows()
        {
            var ops = await _haystackClient.OpsAsync();
            foreach (var row in ops)
            {
                var allKeys = row.Keys;
                var hasVersion = row.ContainsKey("haystackVersion");
                var version = row["haystackVersion"];
                var versionString = (version as HaystackString).Value;
            }
        }

        /// <summary>
        /// Select specific grid rows.
        /// </summary>
        public async Task SelectRows()
        {
            var ops = await _haystackClient.OpsAsync();
            var numRows = ops.RowCount;
            var tenthRow = ops[9];
            var secondRow = ops.Row(1);
        }

        /// <summary>
        /// Get the grid meta data.
        /// </summary>
        public async Task GetMeta()
        {
            var ops = await _haystackClient.OpsAsync();
            var meta = ops.Meta;
            var hasKey = meta.ContainsKey("key");
        }

        /// <summary>
        /// Build a new grid.
        /// </summary>
        public void BuildGrid()
        {
            // Create the grid.
            var grid = new HaystackGrid();

            // Add columns before adding rows.

            // Add a simple column.
            grid.AddColumn("key");
            // Add a column with meta data.
            grid.AddColumn("value", new HaystackDictionary { ["meta"] = new HaystackString("value") });
            // Add a column with a configuration method.
            grid.AddColumn("details", col => col.Meta.AddMarker("isDetails"));

            // Add a row, make sure it has the same number of parameters as there are columns.
            grid.AddRow(new HaystackString("someKey"), new HaystackString("someValue"), new HaystackMarker());
            // Add a row using an array.
            grid.AddRow(new HaystackValue[] { new HaystackString("anotherKey"), new HaystackString("anotherValue"), new HaystackNumber(10, "m") });

            // Add meta data using the default HaystackDictionary functions.
            grid.Meta.AddMarker("marker");
            // Add meta data using the AddMeta helper method.
            grid.AddMeta("key", new HaystackString("value"));
        }

        /// <summary>
        /// Build a new grid using chaining.
        /// </summary>
        public void BuildGridChained()
        {
            // Create the grid.
            var grid = new HaystackGrid()
                .AddColumn("key")
                .AddColumn("value", new HaystackDictionary { ["meta"] = new HaystackString("value") })
                .AddColumn("details", col => col.Meta.AddMarker("isDetails"))
                .AddRow(new HaystackString("someKey"), new HaystackString("someValue"), new HaystackMarker())
                .AddRow(new HaystackValue[] { new HaystackString("anotherKey"), new HaystackString("anotherValue"), new HaystackNumber(10, "m") })
                .AddMeta("marker", new HaystackMarker())
                .AddMeta("key", new HaystackString("value"));
        }
    }
}