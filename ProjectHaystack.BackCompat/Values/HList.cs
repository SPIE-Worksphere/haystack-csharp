using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackList")]
    public class HList : HVal, IEnumerable<HVal>
    {
        public HList(HaystackList source)
        {
            Source = source;
        }
        public HaystackList Source { get; }
        public static HList EMPTY = M.Map(new HaystackList());
        public static HList make(params HVal[] items) => M.Map(new HaystackList(items.Select(M.Map).ToArray()));
        public static HList make(List<HVal> items) => M.Map(new HaystackList(items.Select(M.Map).ToArray()));
        public int size() => Source.Count;
        public HVal get(int i) => M.Map(Source[i]);
        public HVal this[int i] => M.Map(Source[i]);
        public bool CompareItems(List<HVal> items)
        {
            if (items.Count != Source.Count) return false;
            bool bRet = true;
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].hequals(M.Map(Source[i])))
                    bRet = false;
            }
            return bRet;
        }
        public override string toZinc() => ZincWriter.ToZinc(M.Map(this));
        public override string toJson() => HaysonWriter.ToHayson(M.Map(this));
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HList list && Source.Equals(M.Map(list));
        public IEnumerator<HVal> GetEnumerator() => Source.Select(M.Map).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Source.Select(M.Map).GetEnumerator();
    }
}