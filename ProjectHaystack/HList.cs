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
     * HList is a list of HVal items.
     */
    public class HList : HVal
    {
        //////////////////////////////////////////////////////////////////////////
        // Private Constructor
        //////////////////////////////////////////////////////////////////////////
        private HList(HVal[] items)
        {
            if (items != null)
                m_lstItems = new List<HVal>(items);
            else
                m_lstItems = new List<HVal>(); // zero sized list
        }

        public static HList EMPTY = new HList(null);

        //////////////////////////////////////////////////////////////////////////
        // Make Functions
        //////////////////////////////////////////////////////////////////////////
        // Create a list of the given items. The items are copied 
        public static HList make(HVal[] items)
        {
            HVal[] copy = new HVal[items.Length];
            Array.Copy(items, copy, items.Length);
            return new HList(copy);
        }

        // Create a list from the given items. The items are copied 
        public static HList make(List<HVal> items)
        {
            HVal[] copy = items.ToArray();
            return new HList(copy);
        }

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        // Get the number of items in the list 
        public int size() { return m_lstItems.Count; }

        // Get the HVal at the given index 
        public HVal get(int i) { return m_lstItems[i]; }

        public bool CompareItems (List<HVal> items)
        {
            if (items.Count != m_lstItems.Count) return false;
            // Can't compare with Compare or All methods as this does not implement IComparable
            //   to determine equality
            bool bRet = true;
            for (int i = 0; i < items.Count; i++)
            {
                if (!items[i].hequals(m_lstItems[i]))
                    bRet = false;
            }
            return bRet;
        }

        //////////////////////////////////////////////////////////////////////////
        // HVal
        //////////////////////////////////////////////////////////////////////////

        public override string toZinc()
        {
            StringBuilder s = new StringBuilder();
            s.Append('[');
            for (int i = 0; i < m_lstItems.Count; ++i)
            {
                if (i > 0) s.Append(',');
                s.Append(m_lstItems[i].toZinc());
            }
            s.Append(']');
            return s.ToString();
        }

        public override string toJson()
        {
            // YET TO DO - Implement
            throw new NotImplementedException();
        }

        public override bool hequals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType() || !(o is HList)) return false;

            HList ohlist = (HList)o;

            return ohlist.CompareItems(m_lstItems);
        }

        public int hashCode()
        {
            // NOTE: Would not rely on this for unique key - it is too underminable.
            //  this is only left in for documentation.
            return m_lstItems.GetHashCode();
        }

        //////////////////////////////////////////////////////////////////////////
        // List
        //////////////////////////////////////////////////////////////////////////

        private List<HVal> m_lstItems;

    }
}
