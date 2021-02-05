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
    /// <summary>
    /// Haystack dictionary, consists of name/value pairs.
    /// </summary>
    public class HDict : HVal, IDictionary<string, HVal>
    {
        private static HDict _empty = new HDict(new Dictionary<string, HVal>());
        private readonly IDictionary<string, HVal> m_map;

        /// <summary>
        /// Creates an instance of HDict with an initial list of values.
        /// </summary>
        /// <param name="map">Values list.</param>
        public HDict(Dictionary<string, HVal> map)
        {
            m_map = map;
        }

        /// <summary>
        /// Creates an instance of HDict with an initial list of values.
        /// </summary>
        /// <param name="map">Values list.</param>
        public HDict(IDictionary<string, HVal> map)
        {
            m_map = map;
        }

        /// <summary>
        /// A singleton empty HDict.
        /// </summary>
        public static HDict Empty => _empty;

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
        public override int GetHashCode()
            => this.Aggregate(33, (x, kv) => x ^= (kv.Key.GetHashCode() << 7) ^ kv.Value.GetHashCode());

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

        public virtual void Add(string key, HVal value)
        {
            ((IDictionary<string, HVal>)m_map).Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, HVal>)m_map).ContainsKey(key);
        }

        public virtual bool Remove(string key)
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
            foreach (var key in GetKeys())
            {
                array[arrayIndex] = new KeyValuePair<string, HVal>(key, this[key]);
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<string, HVal> item)
        {
            return ((IDictionary<string, HVal>)m_map).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, HVal>> GetEnumerator()
        {
            return GetKeys().Select(key => new KeyValuePair<string, HVal>(key, this[key])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual ICollection<string> GetKeys() => m_map.Keys;

        protected virtual ICollection<HVal> GetValues() => m_map.Values;

        protected virtual HVal GetValue(string key) => m_map.ContainsKey(key) ? m_map[key] : null;

        protected virtual void SetValue(string key, HVal value) => m_map[key] = value;
    }
}