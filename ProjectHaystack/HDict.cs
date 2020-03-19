//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using ProjectHaystack.io;
using System.Collections;

namespace ProjectHaystack
{
    // Remember Haystack definition - "Dict: an associated array of name/value tag pairs"
    public class HDict : HVal, IDictionary<string, HVal>
    {
        // Internal instance of HDict that is of type MapImpl
        //   that is an empty set of tags
        private static HDict m_dictInstanceEmpty = null;
        private static readonly object padlock = new object();
        private Dictionary<string, HVal> m_map;
        private int m_hashCode;
        // Constructor - not a singleton pattern 
        public HDict(Dictionary<string, HVal> map)
        {
            m_hashCode = 0;
            m_map = map;
        }

        // Singleton pattern to single instance of empty
        public static HDict Empty
        {
            get
            {
                lock (padlock)
                {
                    if (m_dictInstanceEmpty == null)
                    {
                        m_dictInstanceEmpty = new HDict(new Dictionary<string, HVal>(11));
                    }
                }
                return m_dictInstanceEmpty;
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Access - these should be overridden by derived classes
        //////////////////////////////////////////////////////////////////////////

        // return number of pairs - should be overriden in derived classes
        public virtual int Size { get { return m_map.Count; } }

        public ICollection<string> Keys => ((IDictionary<string, HVal>)m_map).Keys;

        public ICollection<HVal> Values => ((IDictionary<string, HVal>)m_map).Values;

        public int Count => ((IDictionary<string, HVal>)m_map).Count;

        public bool IsReadOnly => ((IDictionary<string, HVal>)m_map).IsReadOnly;

        public HVal this[string key] { get => ((IDictionary<string, HVal>)m_map)[key]; set => ((IDictionary<string, HVal>)m_map)[key] = value; }

        public virtual int size()
        {
            return Size;
        }

        // Return if size is zero
        public bool isEmpty() { return size() == 0; }
        
        // Return if the given tag is present 
        public bool has(string name) { return get(name, false) != null; }

        // Return if the given tag is not present 
        public bool missing(string name) { return get(name, false) == null; }

        public virtual HVal get(string name) { return get(name, true); }

        public virtual HVal get(string strName, bool bChecked)
        {
            HVal val = null;
            if (m_map.ContainsKey(strName))
                val = m_map[strName];
            if ((val != null) || (!bChecked))
                return val;
            else
                throw new UnknownNameException(strName);

        }

        // NOTE: there is no equivalent of an iterator for a Dictionary object
        //       best equivalent is to access by index hence the next two methods are
        //       a replacement of that

        public HVal getVal(int iIndex, bool bChecked)
        {
            HVal val = null;
            if (iIndex < Size)
            {
                KeyValuePair<string, HVal> item = m_map.ElementAt(iIndex);
                val = item.Value;
            }
            if ((val != null) || (!bChecked))
                return val;
            else
                throw new IndexOutOfRangeException(iIndex.ToString() + " out of range");
        }

        public virtual string getKeyAt(int iIndex, bool bChecked)
        {
            string strRet = null;
            if (iIndex < Size)
            {
                KeyValuePair<string, HVal> item = m_map.ElementAt(iIndex);
                strRet = item.Key;
            }
            if ((strRet != null) || (!bChecked))
                return strRet;
            else
                throw new IndexOutOfRangeException(iIndex.ToString() + " out of range");
        }

        // access HRef
        public HRef id() { return getRef("id"); }

        // dis 
        public string dis()
        {
            HVal v;
            v = get("dis", false); if (v is HStr) return ((HStr)v).Value;
            v = get("id", false); if (v != null) return ((HRef)v).display();
            return "????";
        }

        //////////////////////////////////////////////////////////////////////////
        // Get Conveniences
        //////////////////////////////////////////////////////////////////////////
        // Get tag as HBool or raise UnknownNameException or ClassCastException. 
        public bool getBool(string name) { return ((HBool)get(name)).val; }

        // Get tag as HStr or raise UnknownNameException or ClassCastException. 
        public string getStr(string name) { return ((HStr)get(name)).Value; }

        // Get tag as HRef or raise UnknownNameException or ClassCastException. 
        public HRef getRef(string name) { return (HRef)get(name); }

        // Get tag as HNum or raise UnknownNameException or ClassCastException. 
        public int getInt(string name) { return (int)((HNum)get(name)).doubleval; }

        // Get tag as HNum or raise UnknownNameException or ClassCastException. 
        public double getDouble(string name) { return ((HNum)get(name)).doubleval; }

        //////////////////////////////////////////////////////////////////////////
        // Identity
        //////////////////////////////////////////////////////////////////////////
        // String format is always "toZinc" 
        public string toString() { return toZinc(); }

        // Hash code is based on tags 
        public int hashCode()
        {
            if (m_hashCode == 0)
            {
                int x = 33;
                for (int it = 0; it < size(); it++)
                {
                    string key = getKeyAt(it, false);
                    HVal val = getVal(it, false);
                    /*Entry entry = (Entry)it.next();
                    Object key = entry.getKey();
                    Object val = entry.getValue();*/
                    if (val != null) 
                        x ^= (key.GetHashCode() << 7) ^ val.GetHashCode();
                }
                m_hashCode = x;
            }
            return m_hashCode;
        }

        // Equality is tags same Dict with same contents
        public override bool hequals(object that)
        {
            if (!(that is HDict)) return false;
            HDict x = (HDict)that;
            if (size() != x.size()) return false;
            for (int it = 0; it < size(); it++)
            {
                string key = getKeyAt(it, false);
                HVal val = getVal(it, false);
                if (val != null)
                {
                    if (!val.hequals(x.get(key, false)))
                        return false;
                }
            }
            return true;
        }
        //////////////////////////////////////////////////////////////////////////
        // Encoding
        //////////////////////////////////////////////////////////////////////////

        /*
         * Return if the given string is a legal tag name.  The
         * first char must be ASCII lower case letter.  Rest of
         * chars must be ASCII letter, digit, or underbar.
         */
         // Replaced with Regex - a lot simplier implementation in regular expressions
         //   and less looping. Got to love .NET !
        public static bool isTagName(string n)
        {
            return n != null && Regex.IsMatch(n, @"^[a-z^][a-zA-Z0-9_]*$");
        }

        //////////////////////////////////////////////////////////////////////////
        // HVal
        //////////////////////////////////////////////////////////////////////////

        // Encode value to zinc format 
        public override string toZinc()
        {
            return HZincWriter.valToString(this);
        }

        // Encode value to json format 
        public override string toJson()
        {
            return HJsonWriter.valToString(this);
        }

        public void Add(string key, HVal value)
        {
            ((IDictionary<string, HVal>)m_map).Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, HVal>)m_map).ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, HVal>)m_map).Remove(key);
        }

        public bool TryGetValue(string key, out HVal value)
        {
            return ((IDictionary<string, HVal>)m_map).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, HVal> item)
        {
            ((IDictionary<string, HVal>)m_map).Add(item);
        }

        public void Clear()
        {
            ((IDictionary<string, HVal>)m_map).Clear();
        }

        public bool Contains(KeyValuePair<string, HVal> item)
        {
            return ((IDictionary<string, HVal>)m_map).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, HVal>[] array, int arrayIndex)
        {
            ((IDictionary<string, HVal>)m_map).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, HVal> item)
        {
            return ((IDictionary<string, HVal>)m_map).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, HVal>> GetEnumerator()
        {
            return ((IDictionary<string, HVal>)m_map).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, HVal>)m_map).GetEnumerator();
        }

        //////////////////////////////////////////////////////////////////////////
        // MapImpl - I see no benefit to this and is not defined in Haystack
        //            .NET has keyValuePair if needed and suspect the toEntry
        //            static entry was to satisfy Java hashmap not needed with
        //            a .NET Dictionairy object.  Still Singleton above just removed
        //            the unnecessary subclass.
        //////////////////////////////////////////////////////////////////////////

        /*
        // Can't be static and derive from hDict
        public class MapImpl : HDict
        {
            //private Dictionary<string, HVal> m_map;
            // Dictionary is a close but not exact match to a java hash map
            public MapImpl (Dictionary<string, HVal> map)
            {
                m_map = map;
            }


        }*/
    }
}
