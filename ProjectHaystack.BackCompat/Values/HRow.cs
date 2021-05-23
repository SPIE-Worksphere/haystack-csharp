using System;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackRow")]
    public class HRow : HDict
    {
        public HRow(HaystackRow source)
            : base(source)
        {
            Source = source;
        }
        public HaystackRow Source { get; }
        public HGrid Grid => M.Map(Source.Grid);
        public HDict ToDict() => M.Map(Source);
        public override void Add(string key, HVal value) => Source.Add(key, M.Map(value));
        public override bool Remove(string key) => Source.Remove(key);
    }
}