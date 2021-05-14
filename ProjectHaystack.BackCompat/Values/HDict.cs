using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectHaystack.io;
using ProjectHaystack.Validation;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackDictionary")]
    public class HDict : HVal, IDictionary<string, HVal>
    {
        public HDict(Dictionary<string, HVal> map)
        {
            Source = new HaystackDictionary(map.ToDictionary(kv => kv.Key, kv => M.Map(kv.Value)));
        }
        public HDict(IDictionary<string, HVal> map)
        {
            Source = new HaystackDictionary(map.ToDictionary(kv => kv.Key, kv => M.Map(kv.Value)));
        }
        public HDict(HaystackDictionary source)
        {
            Source = source;
        }
        public HaystackDictionary Source { get; }

        public static HDict Empty => new HDict(new HaystackDictionary());
        public virtual int Size => Source.Count;
        public ICollection<string> Keys => Source.Keys;
        public ICollection<HVal> Values => Source.Values.Select(M.Map).ToArray();
        public int Count => Source.Count;
        public virtual bool IsReadOnly => Source.IsReadOnly;
        public HVal this[string key] { get => M.Map(Source[key]); set => Source[key] = M.Map(value); }
        public virtual int size() => Source.Count;
        public bool isEmpty() => Source.IsEmpty();
        public bool has(string name) => Source.ContainsKey(name);
        public bool missing(string name) => !Source.ContainsKey(name);
        public virtual HVal get(string name) => M.Map(Source.Get(name));
        public virtual HVal get(HCol col, bool bChecked) => M.Checked(() => M.Map(Source.Get(col.Name)), bChecked);
        public virtual HVal get(string strName, bool bChecked) => M.Checked(() => M.Map(Source.Get(strName)), bChecked);
        // TODO
        public HVal getVal(int iIndex, bool bChecked) => M.Checked(() => M.Map(Source.Get(Source.Keys.Skip(iIndex).First())), bChecked);
        public virtual string getKeyAt(int iIndex, bool bChecked) => M.Checked(() => Source.Keys.Skip(iIndex).First(), bChecked);
        public HRef id() => M.Map(Source.GetReference("id"));
        public string dis() => Source.Display;
        public bool getBool(string name) => Source.GetBoolean(name);
        public string getStr(string name) => Source.GetString(name);
        public HRef getRef(string name) => M.Map(Source.GetReference(name));
        public int getInt(string name) => (int)Source.GetDouble(name);
        public double getDouble(string name) => Source.GetDouble(name);
        public HDef getDef(string name) => M.Map(Source.Get<HaystackDefinition>(name));
        public string toString() => ZincWriter.ToZinc(Source);
        public override int GetHashCode() => Source.GetHashCode();
        public override bool Equals(object that) => that != null && that is HDict dict && Source.Equals(M.Map(dict));
        public static bool isTagName(string n) => HaystackValidator.IsTagName(n);
        public override string toZinc() => ZincWriter.ToZinc(Source);
        public override string toJson() => HaysonWriter.ToHayson(Source);
        public virtual void Add(string key, HVal value) => Source.Add(key, M.Map(value));
        public bool ContainsKey(string key) => Source.ContainsKey(key);
        public virtual bool Remove(string key) => Source.Remove(key);
        public bool TryGetValue(string key, out HVal value)
        {
            if (Source.TryGetValue(key, out var val))
            {
                value = M.Map(val);
                return true;
            }
            value = null;
            return false;
        }
        public void Add(KeyValuePair<string, HVal> item) => Source.Add(item.Key, M.Map(item.Value));
        public void Clear() => Source.Clear();
        public bool Contains(KeyValuePair<string, HVal> item) => Source.Contains(new KeyValuePair<string, HaystackValue>(item.Key, M.Map(item.Value)));
        public void CopyTo(KeyValuePair<string, HVal>[] array, int arrayIndex)
            => Source.Select(kv => new KeyValuePair<string, HVal>(kv.Key, M.Map(kv.Value))).ToArray().CopyTo(array, arrayIndex);
        public bool Remove(KeyValuePair<string, HVal> item) => Source.Remove(new KeyValuePair<string, HaystackValue>(item.Key, M.Map(item.Value)));
        public IEnumerator<KeyValuePair<string, HVal>> GetEnumerator() => Source.Select(kv => new KeyValuePair<string, HVal>(kv.Key, M.Map(kv.Value))).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}