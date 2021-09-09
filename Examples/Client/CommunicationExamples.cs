using System.Linq;
using System.Threading.Tasks;
using ProjectHaystack.Client;

namespace ProjectHaystack.Examples.Client
{
    /// <summary>
    /// Examples on how to communicate with the Haystack server.
    /// </summary>
    public class CommunicationExamples
    {
        private readonly IHaystackClient _haystackClient;

        public CommunicationExamples(IHaystackClient haystackClient)
        {
            _haystackClient = haystackClient;
        }

        /// <summary>
        /// Get the about page and read its version.
        /// </summary>
        public async Task About()
        {
            var about = await _haystackClient.AboutAsync();
            var version = about.GetString("haystackVersion");
        }

        /// <summary>
        /// Get the ops page and read its capabilities.
        /// </summary>
        public async Task Ops()
        {
            var ops = await _haystackClient.OpsAsync();
            var capabilities = ops.ToDictionary(row => row.GetString("name"), row => row.GetString("summary"));
        }

        /// <summary>
        /// Do a raw call that will not be parsed.
        /// This is useful if you mainly want to use the authentication mechanism,
        /// but not the data conversion and do conversion elsewhere.
        /// You can use this to get specific data types from the server, like json.
        /// </summary>
        public async Task Raw()
        {
            string aboutAsJson = await _haystackClient.PostStringAsync("about", string.Empty, "text/zinc", "application/json");
        }

        /// <summary>
        /// When sending more complex requests, like an Axon query in SkySpark
        /// you can build and send a grid with a specific operation.
        /// </summary>
        public async Task GridQuery()
        {
            var axon = "someaxon";
            var grid = new HaystackGrid()
                .AddColumn("expr")
                .AddRow(new HaystackString(axon));
            HaystackGrid[] result = await _haystackClient.EvalAllAsync(grid);
        }
    }
}