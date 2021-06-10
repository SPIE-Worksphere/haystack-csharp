namespace ProjectHaystack
{
    public static class HaystackDictionaryExtensions
    {
        public static TValue GetUnchecked<TValue>(this HaystackDictionary dict, string name)
            where TValue : HaystackValue
        {
            if (dict.ContainsKey(name))
                return dict.Get<TValue>(name);
            return null;
        }

        public static string GetString(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackString>(name).Value;
        }

        public static string GetStringUnchecked(this HaystackDictionary dict, string name)
        {
            return dict.GetUnchecked<HaystackString>(name)?.Value;
        }

        public static bool GetBoolean(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackBoolean>(name).Value;
        }

        public static bool GetBooleanUnchecked(this HaystackDictionary dict, string name)
        {
            return dict.GetUnchecked<HaystackBoolean>(name)?.Value ?? false;
        }

        public static HaystackReference GetReference(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackReference>(name);
        }

        public static HaystackReference GetReferenceUnchecked(this HaystackDictionary dict, string name)
        {
            return dict.GetUnchecked<HaystackReference>(name);
        }

        public static double GetDouble(this HaystackDictionary dict, string name)
        {
            return dict.Get<HaystackNumber>(name).Value;
        }

        public static double GetDoubleUnchecked(this HaystackDictionary dict, string name)
        {
            return dict.GetUnchecked<HaystackNumber>(name)?.Value ?? 0;
        }
    }
}