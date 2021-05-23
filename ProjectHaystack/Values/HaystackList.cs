using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectHaystack
{
    /// <summary>
    /// List of Haystack values.
    /// </summary>
    public class HaystackList : HaystackValue, IList<HaystackValue>
    {
        private readonly List<HaystackValue> _list;

        public HaystackList(List<HaystackValue> list)
        {
            _list = list;
        }

        public HaystackList(params HaystackValue[] list)
        {
            _list = list.ToList();
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public HaystackValue this[int index] { get => _list[index]; set => _list[index] = value; }

        public override int GetHashCode() => _list.GetHashCode();

        public override bool Equals(object other)
        {
            return other != null
                && other is HaystackList list
                && list._list.Count == _list.Count
                && Enumerable.Range(0, list._list.Count).All(idx => list._list[idx].Equals(_list[idx]));
        }

        public int IndexOf(HaystackValue item) => _list.IndexOf(item);

        public void Insert(int index, HaystackValue item) => _list.Insert(index, item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        public void Add(HaystackValue item) => _list.Add(item);

        public void Clear() => _list.Clear();

        public bool Contains(HaystackValue item) => _list.Contains(item);

        public void CopyTo(HaystackValue[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public bool Remove(HaystackValue item) => _list.Remove(item);

        public IEnumerator<HaystackValue> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}