using System.Threading.Tasks;
using ProjectHaystack.Client;

namespace ProjectHaystack.Examples.Values
{

    /// <summary>
    /// Examples on how to use a HaystackDictionary.
    /// Note that the values inherit HaystackValue,
    /// so they should be cast to the expected type to access their values.
    /// The ProjectHaystack namespace contains various convenience extension methods
    /// for common operations, like reading a string value.
    /// </summary>
    public class HaystackDictionaryExamples
    {
        private readonly IHaystackClient _haystackClient;

        public HaystackDictionaryExamples(IHaystackClient haystackClient)
        {
            _haystackClient = haystackClient;
        }

        /// <summary>
        /// Various ways to get values from the dictionary.
        /// </summary>
        public async Task GetValues()
        {
            var about = await _haystackClient.AboutAsync();

            // Getting values using the indexer or Get methods.
            var value0 = about["key0"];
            var value1 = about.Get("key1");
            var value2 = about.Get<HaystackString>("key2");
            // Accessing any key that does not exist will throw an exception,
            // so make sure to check if it exists when you are not sure.
            var value3 = about.ContainsKey("key3") ? about.Get("key3") : null;

            // GetString is a convenience method that expects the value
            // to be a HaystackString, casts it and gets its value.
            var value4 = about.GetString("key4");

            // All *Unchecked convenience methods will not throw an exception
            // when the key does not exist, but will return the default value instead.
            var value5 = about.GetStringUnchecked("key5");
        }

        /// <summary>
        /// Various ways to build up a new dictionary.
        /// </summary>
        public void BuildDictionary()
        {
            // Build a dictionary and add values as you would a normal Dictionary.
            var dictionary0 = new HaystackDictionary();
            dictionary0.Add("key", new HaystackString("value"));
            dictionary0.AddMarker("someMarker");
            dictionary0.AddNumber("someNumber", 10, "m");

            // Build a dictionary and remove values as you would a normal Dictionary.
            var dictionary1 = new HaystackDictionary
            {
                ["key"] = new HaystackString("value"),
                ["someMarker"] = new HaystackMarker(),
            };
            dictionary1.Remove("someMarker");
        }
    }
}