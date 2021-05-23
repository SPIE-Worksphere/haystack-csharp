namespace ProjectHaystack
{
    public static class HaystackDictionaryExtensions
    {
        public static string GetString(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackString>(name).Value;
        }

        public static bool GetBoolean(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackBoolean>(name).Value;
        }

        public static HaystackReference GetReference(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackReference>(name);
        }

        public static double GetDouble(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackNumber>(name).Value;
        }
    }
}