using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProjectHaystack
{
    /// <summary>
    /// Haystack dictionary, consists of name/value pairs.
    /// </summary>
    public class HaystackDictionary : HaystackValue, IDictionary<string, HaystackValue>
    {
        protected readonly Lazy<IDictionary<string, HaystackValue>> _source;

        public HaystackDictionary()
            : this(new Dictionary<string, HaystackValue>())
        {
        }

        /// <summary>
        /// Creates an instance of HaystackDictionary with an initial list of values.
        /// </summary>
        /// <param name="values">Values list.</param>
        public HaystackDictionary(IDictionary<string, HaystackValue> values)
#if NETSTANDARD2_0 || NETSTANDARD2_1
            : this(new Lazy<IDictionary<string, HaystackValue>>(() => values.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value)))
#else
            : this(new Lazy<IDictionary<string, HaystackValue>>(new Dictionary<string, HaystackValue>(values.Where(kv => kv.Value != null))))
#endif
        {
        }

        protected HaystackDictionary(Lazy<IDictionary<string, HaystackValue>> source)
        {
            _source = source;
        }

        public int Count => _source.Value.Count;

        public HaystackReference Id => (HaystackReference)_source.Value["id"];

        public ICollection<string> Keys => _source.Value.Keys;

        public virtual ICollection<HaystackValue> Values => _source.Value.Values;

        public bool IsReadOnly => false;

        public virtual HaystackValue this[string key] { get => Get(key); set => _source.Value[key] = value; }

        public bool IsEmpty() => Count == 0;

        public virtual HaystackValue Get(string name)
        {
            return _source.Value.ContainsKey(name)
                ? _source.Value[name] ?? throw new HaystackUnknownNameException(name)
                : throw new HaystackUnknownNameException(name);
        }

        public TValue Get<TValue>(string name)
            where TValue : HaystackValue
        {
            return (TValue)Get(name);
        }

        public string Display
        {
            get
            {
                if (_source.Value.ContainsKey("dis") && _source.Value["dis"] is HaystackString str)
                {
                    return str.Value;
                }
                if (_source.Value.ContainsKey("id") && _source.Value["id"] is HaystackReference @ref)
                {
                    return @ref.Display ?? @ref.Value;
                }
                throw new InvalidOperationException($"Cannot get display value");
            }
        }

        public override int GetHashCode() => _source.Value.GetHashCode();

        public override bool Equals(object other)
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
            if (other == null || !(other is HaystackDictionary dict) || _source.Value.Count != dict.Count)
            {
                return false;
            }
#else
            if (other == null || other is not HaystackDictionary dict || _source.Value.Count != dict.Count)
            {
                return false;
            }
#endif
            foreach (var key in Keys)
            {
                if (!dict.ContainsKey(key) || !dict[key].Equals(this[key]))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void Add(string key, HaystackValue value)
        {
            if (value != null)
            {
                _source.Value.Add(key, value);
            }
        }

#region Convenience methods

        public HaystackDictionary AddValue(string key, HaystackValue value)
        {
            Add(key, value);
            return this;
        }
        public HaystackDictionary AddString(string key, string value)
        {
            Add(key, new HaystackString(value));
            return this;
        }
        public HaystackDictionary AddNumber(string key, double value, string unit = null)
        {
            Add(key, new HaystackNumber(value, unit));
            return this;
        }
        public HaystackDictionary AddMarker(string key)
        {
            Add(key, new HaystackMarker());
            return this;
        }

#endregion Convenience methods

        public bool ContainsKey(string key) => _source.Value.ContainsKey(key) && _source.Value[key] != null;

        public virtual bool Remove(string key) => _source.Value.Remove(key);

        public bool TryGetValue(string key, out HaystackValue value) => _source.Value.TryGetValue(key, out value);

        public void Add(KeyValuePair<string, HaystackValue> item) => _source.Value.Add(item);

        public void Clear() => _source.Value.Clear();

        public bool Contains(KeyValuePair<string, HaystackValue> item) => _source.Value.Contains(item);

        public void CopyTo(KeyValuePair<string, HaystackValue>[] array, int arrayIndex) => _source.Value.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<string, HaystackValue> item) => _source.Value.Remove(item);

        public IEnumerator<KeyValuePair<string, HaystackValue>> GetEnumerator() => _source.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _source.Value.GetEnumerator();
    }
}