//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    /**
     * HDictBuilder is used to construct a HDict instance. (Java had this as immutable - but I don't see why this is crticial)
     *
     * @see <a href='http://project-haystack.org/doc/TagModel#tags'>Project Haystack</a>
     */
    public class HDictBuilder
    {
        private Dictionary<string, HVal> m_map;

        public HDictBuilder ()
        {
            m_map = null;
        }
        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        // Return if size is zero 
        public bool isEmpty() { return size() == 0; }

        // Return number of tag name/value pairs 
        public int size() { return m_map.Count; }

        // Return if the given tag is present 
        public bool has(string name) { return get(name, false) != null; }

        // Return if the given tag is not present 
        public bool missing(string name) { return get(name, false) == null; }

        // Convenience for "get(name, true)" 
        public HVal get(string name) { return get(name, true); }

        // Get a tag by name.  If not found and checked if false then
        //    return null, otherwise throw UnknownNameException 
        public HVal get(string name, bool bchecked)
        {
            HVal val = null;
            if (m_map.ContainsKey(name))
                val = m_map[name];
            if (val != null) return val;
            if (!bchecked) return null;
            throw new UnknownNameException(name);
        }

        //////////////////////////////////////////////////////////////////////////
        // Utils
        //////////////////////////////////////////////////////////////////////////

        // Convenience for <code>add(name, HMarker.VAL)</code> 
        public HDictBuilder add(string name)
        {
            return add(name, HMarker.VAL); 
        }

        // Convenience for <code>add(name, HBool.make(val))</code> 
        public HDictBuilder add(string name, bool val)
        {
            return add(name, HBool.make(val)); 
        }

        // Convenience for <code>add(name, HNum.make(val))</code> 
        public HDictBuilder add(string name, long val)
        {
            return add(name, HNum.make(val));
        }

        // Convenience for <code>add(name, HNum.make(val, unit))</code> 
        public HDictBuilder add(string name, long val, String unit)
        {
            return add(name, HNum.make(val, unit));
        }

        // Convenience for <code>add(name, HNum.make(val))</code> 
        public HDictBuilder add(string name, double val)
        {
            return add(name, HNum.make(val));
        }

        // Convenience for <code>add(name, HNum.make(val, unit))</code> 
        public HDictBuilder add(string name, double val, string unit)
        {
            return add(name, HNum.make(val, unit));
        }

        // Convenience for <code>add(name, HStr.make(val))</code> 
        public HDictBuilder add(string name, string val)
        {
            return add(name, HStr.make(val));
        }

        // Add all the name/value pairs in given HDict.  Return this. 
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

        // Add tag name and value.  Return this. 
        public HDictBuilder add(string name, HVal val)
        {
            if (!HDict.isTagName(name))
                throw new InvalidOperationException("Invalid tag name: " + name);
            if (m_map == null) m_map = new Dictionary<string, HVal>();
            m_map.Add(name, val);
            return this;
        }

        // Convert current state to an immutable HDict instance 
        public HDict toDict()
        {
            if (m_map == null || m_map.Count == 0) return HDict.Empty;
            HDict dict = new HDict(m_map);
            m_map = null;
            return dict;
        }
    }
}
