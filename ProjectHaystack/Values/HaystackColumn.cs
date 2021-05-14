namespace ProjectHaystack
{
    /// <summary>
    /// Haystack column information.
    /// </summary>
    public class HaystackColumn
    {
        public HaystackColumn(int index, string name, HaystackDictionary meta = null)
        {
            Index = index;
            Name = name;
            Meta = meta ?? new HaystackDictionary();
        }

        public int Index { get; }
        public string Name { get; }
        public HaystackDictionary Meta { get; }

        public string Display
        {
            get
            {
                HaystackValue dis = Meta.ContainsKey("dis") ? Meta["dis"] : null;
                return dis is HaystackString str ? str.Value : Name;
            }
        }

        public override int GetHashCode() => Name.GetHashCode() ^ Meta.GetHashCode();

        public override bool Equals(object other) => other != null && other is HaystackColumn col && Name.Equals(col.Name) && Meta.Equals(col.Meta);
    }
}