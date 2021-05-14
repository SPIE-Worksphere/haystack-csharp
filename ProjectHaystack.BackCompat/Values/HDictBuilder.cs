using System;
using System.Collections.Generic;

namespace ProjectHaystack
{
    [Obsolete("Use HaystackDict")]
    public class HDictBuilder
    {
        private Dictionary<string, HVal> m_map;

        public HDictBuilder ()
        {
            m_map = null;
        }
        public bool isEmpty() { return size() == 0; }
        public int size() { return m_map.Count; }
        public bool has(string name) { return get(name, false) != null; }
        public bool missing(string name) { return get(name, false) == null; }
        public HVal get(string name) { return get(name, true); }
        public HVal get(string name, bool bchecked)
        {
            HVal val = null;
            if (m_map.ContainsKey(name))
                val = m_map[name];
            if (val != null) return val;
            if (!bchecked) return null;
            throw new HaystackUnknownNameException(name);
        }
        public HDictBuilder add(string name)
        {
            return add(name, HMarker.VAL); 
        }
        public HDictBuilder add(string name, bool val)
        {
            return add(name, HBool.make(val)); 
        }
        public HDictBuilder add(string name, long val)
        {
            return add(name, HNum.make(val));
        }
        public HDictBuilder add(string name, long val, String unit)
        {
            return add(name, HNum.make(val, unit));
        }
        public HDictBuilder add(string name, double val)
        {
            return add(name, HNum.make(val));
        }
        public HDictBuilder add(string name, double val, string unit)
        {
            return add(name, HNum.make(val, unit));
        }
        public HDictBuilder add(string name, string val)
        {
            return add(name, HStr.make(val));
        }
        public HDictBuilder add(HDict dict)
        {
            for (int it = 0; it < dict.size(); it++)
            {
                string strKey = dict.getKeyAt(it, false);
                if (strKey != null)
                add(strKey, (HVal)dict.get(strKey,false));
            }
            return this;
        }
        public HDictBuilder add(string name, HVal val)
        {
            if (!HDict.isTagName(name))
                throw new InvalidOperationException("Invalid tag name: " + name);
            if (m_map == null) m_map = new Dictionary<string, HVal>();
            m_map.Add(name, val);
            return this;
        }
        public HDict toDict()
        {
            if (m_map == null || m_map.Count == 0) return HDict.Empty;
            HDict dict = new HDict(m_map);
            m_map = null;
            return dict;
        }
    }
}