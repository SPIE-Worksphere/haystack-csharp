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
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack
{
    public class HCol
    {
        private int m_iIndex;
        private string m_strName;
        private HDict m_dictMeta;
        // Constructor
        public HCol (int iIndex, string name, HDict meta)
        {
            m_iIndex = iIndex;
            m_strName = name;
            m_dictMeta = meta;
        }
        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        // Return programatic name of column 
        public string Name { get { return m_strName; } }

        // Return display name of column which is meta.dis or name
        public string dis()
        {
            HVal dis = m_dictMeta.get("dis", false);
            if (dis is HStr) return ((HStr)dis).Value;
            return m_strName;
        }

        // Column meta-data tags 
        public HDict meta { get { return m_dictMeta; } }

        // Access Index 
        public int Index { get { return m_iIndex; } }
        
        //////////////////////////////////////////////////////////////////////////
        // Identity
        //////////////////////////////////////////////////////////////////////////

        // Hash code is not needed for .NET - see notes in HDict

        // Equality is name and meta 
        public bool hequals(object that)
        {
            if (!(that is HCol)) return false;
            HCol x = (HCol)that;
            return ((m_strName.Equals(x.Name)) && (meta.hequals(x.meta)));
        }

    }
}
