//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ProjectHaystack.io;

namespace ProjectHaystack
{
    // Remember Haystack definition - "Dict: an associated array of name/value tag pairs"
    public class HDict : HVal, IDictionary<string, HVal>
    {
        // Internal instance of HDict that is of type MapImpl
        //   that is an empty set of tags
        private static readonly object padlock = new object();
        private readonly Dictionary<string, HVal> m_map;
        private Lazy<int> m_hashCode;

        // Constructor - not a singleton pattern 
        public HDict(Dictionary<string, HVal> map)
        {
            m_map = map;
            m_hashCode = new Lazy<int>(()
                => this.Aggregate(33, (x, kv) => x ^= (kv.Key.GetHashCode() << 7) ^ kv.Value.GetHashCode()));
        }

        // Singleton pattern to single instance of empty
        public static HDict Empty
        {
            get
            {
                return new HDict(new Dictionary<string, HVal>());
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Access - these should be overridden by derived classes
        //////////////////////////////////////////////////////////////////////////

        // return number of pairs - should be overriden in derived classes
        public virtual int Size => GetValues().Count;

        public ICollection<string> Keys => GetKeys();

        public ICollection<HVal> Values => GetValues();

        public int Count => Size;

        public virtual bool IsReadOnly => ((ICollection<HVal>)m_map).IsReadOnly;

        public HVal this[string key] { get => GetValue(key); set => SetValue(key, value); }

        public virtual int size() => Size;

        // Return if size is zero
        public bool isEmpty() => Size == 0;

        // Return if the given tag is present 
        public bool has(string name) => get(name, false) != null;

        // Return if the given tag is not present 
        public bool missing(string name) => !has(name);

        public virtual HVal get(string name) => get(name, true);

        public virtual HVal get(HCol col, bool bChecked) => get(col.Name, bChecked);

        public virtual HVal get(string strName, bool bChecked)
        {
            HVal val = GetValue(strName);
            return val != null || !bChecked
                ? val
                : throw new UnknownNameException(strName);
        }

        public HVal getVal(int iIndex, bool bChecked)
        {
            if ((iIndex < 0 || iIndex > Size) && bChecked)
                throw new IndexOutOfRangeException(iIndex.ToString() + " out of range");
            return get(Keys.ElementAt(iIndex), bChecked);
        }

        public virtual string getKeyAt(int iIndex, bool bChecked)
        {
            if (iIndex < 0 || iIndex > Size)
                if (bChecked)
                    throw new IndexOutOfRangeException(iIndex.ToString() + " out of range");
                else
                    return null;
            return Keys.ElementAt(iIndex);
        }

        // access HRef
        public HRef id() => getRef("id");

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
        public bool getBool(string name) => ((HBool)get(name)).val;

        // Get tag as HStr or raise UnknownNameException or ClassCastException. 
        public string getStr(string name) => ((HStr)get(name)).Value;

        // Get tag as HRef or raise UnknownNameException or ClassCastException. 
        public HRef getRef(string name) => (HRef)get(name);

        // Get tag as HNum or raise UnknownNameException or ClassCastException. 
        public int getInt(string name) => (int)((HNum)get(name)).doubleval;

        // Get tag as HNum or raise UnknownNameException or ClassCastException. 
        public double getDouble(string name) => ((HNum)get(name)).doubleval;

        public HDef getDef(string name) => ((HDef)get(name));

        //////////////////////////////////////////////////////////////////////////
        // Identity
        //////////////////////////////////////////////////////////////////////////
        // String format is always "toZinc" 
        public string toString() { return toZinc(); }

        // Hash code is based on tags 
        public override int GetHashCode() => m_hashCode.Value;

        // Equality is tags same Dict with same contents
        public override bool Equals(object that)
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

        protected virtual ICollection<string> GetKeys() => m_map.Keys;

        protected virtual ICollection<HVal> GetValues() => m_map.Values;

        protected virtual HVal GetValue(string key) => m_map.ContainsKey(key) ? m_map[key] : null;

        protected virtual void SetValue(string key, HVal value) => m_map[key] = value;


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
