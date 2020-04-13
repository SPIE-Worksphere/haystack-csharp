//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   1 April 2018   Ian Davies  Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ProjectHaystack.io;

namespace ProjectHaystack
{
    // NOTE (implementation difference from java toolkit): All basic arrays replaced 
    //   by List - Prefer to use one type and not mix.  If we are using basic arrays 
    //   then they should be used everywhere, if we are using generic collections 
    //   then we should use them everywhere.
    public class HGrid : HVal, IEnumerable<HRow>
    {
        private List<HCol> m_cols;
        private List<HRow> m_rows;
        private Dictionary<string, HCol> m_colsByName;
        // Singleton empty instance
        private static HGrid m_empty;

        public static HGrid InstanceEmpty
        {
            get
            {
                if (m_empty == null)
                {
                    List<HCol> col = new List<HCol>();
                    col.Add(new HCol(0, "empty", HDict.Empty));
                    m_empty = new HGrid(HDict.Empty, col, new List<List<HVal>>());
                }
                return m_empty;
            }
        }

        // Constructor should be within this class library only
        internal HGrid(HDict meta, List<HCol> cols, List<List<HVal>> rowLists)
        {
            this.meta = meta ?? throw new ArgumentNullException("metadata cannot be null");

            // number of cells in rows must match size of cols
            int iIndex = 0;
            foreach (List<HVal> curRow in rowLists)
            {
                if (curRow != null)
                {
                    if (curRow.Count != cols.Count)
                        throw new ArgumentException("row cells at " + iIndex.ToString() + " does not match col size of " + cols.Count.ToString(), "rowLists");
                }
                iIndex++;
            }
            // Each Col must have a unique name
            Dictionary<string, bool> tempDict = new Dictionary<string, bool>();
            foreach (HCol curCol in cols)
            {
                if (curCol != null)
                {
                    if (tempDict.ContainsKey(curCol.Name))
                        throw new ArgumentException("column with name " + curCol.Name + " is duplicated");
                }
            }

            m_cols = cols;
            m_rows = new List<HRow>();
            foreach (List<HVal> curRow in rowLists)
            {
                m_rows.Add(new HRow(this, curRow));
            }
            m_colsByName = new Dictionary<string, HCol>();
            foreach (HCol curCol in cols)
            {
                m_colsByName.Add(curCol.Name, curCol);
            }
        }

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////

        // Return grid level meta 
        public HDict meta { get; }

        // Error grid have the meta.err marker tag
        public bool isErr() { return meta.has("err"); }

        // Return if number of rows is zero 
        public bool isEmpty() { return numRows == 0; }

        // Return number of rows 
        public int numRows
        {
            get { return m_rows.Count; }
        }

        // Get a row by its zero based index 
        public HRow row(int row) { return m_rows[row]; }

        // Get number of columns  
        public int numCols
        {
            get { return m_cols.Count; }
        }

        // Get a column by its index 
        public HCol col(int index) { return m_cols[index]; }

        // Convenience for "col(name, true)" 
        public HCol col(string name) { return col(name, true); }

        // Get a column by name.  If not found and checked if false then
        //    return null, otherwise throw UnknownNameException 
        public HCol col(string name, bool bchecked)
        {
            HCol colRet = null;
            if (m_colsByName.ContainsKey(name))
                colRet = m_colsByName[name];
            if (colRet != null) return colRet;
            if (bchecked) throw new UnknownNameException(name);
            return null;
        }

        // HDict needed here
        // Create iteratator to walk each row  - Removed as it is not needed 
        /*public IEnumerable<HRow> iterator()
        {
            return (m_rows.AsEnumerable());
        }*/

        //////////////////////////////////////////////////////////////////////////
        // HVal
        //////////////////////////////////////////////////////////////////////////

        public override string toZinc()
        {
            return (HZincWriter.valToString(this));
        }

        public override string toJson()
        {
            return (HJsonWriter.valToString(this));
        }

        public override bool hequals(object o)
        {
            // Instance check
            if (o.Equals(this)) return true;
            // null and unlike type check
            if (o == null || GetType() != o.GetType()) return false;
            // further checks require it is of type HGrid
            if (!(o is HGrid)) return false;
            HGrid gridO = (HGrid)o;
            // Compare Meta
            if (!meta.hequals(gridO.meta)) return false;
            // Compare Cols - don't like the java implementation
            if (numCols != gridO.numCols) return false;
            for (int iCurCol = 0; iCurCol < numCols; iCurCol++)
                if (!col(iCurCol).hequals(gridO.col(iCurCol)))
                    return false;
            // Compare Rows - don't like the java implementation
            if (numRows != gridO.numRows) return false;
            for (int iCurRow = 0; iCurRow < numRows; iCurRow++)
                if (!row(iCurRow).hequals(gridO.col(iCurRow)))
                    return false;
            return true;
        }

        // As per HDict comments .NET does not need a hashcode
        //////////////////////////////////////////////////////////////////////////
        // Debug
        //////////////////////////////////////////////////////////////////////////

        // .NET Implementation writing to Debug out
        public void dump()
        {
            Debug.WriteLine(HZincWriter.gridToString(this));
            Debug.Flush();
        }
        // .NET Implementation writing to stdout (Console)
        public void dumpToConsole()
        {
            Console.WriteLine(HZincWriter.gridToString(this));
        }
        // .NET Implementation in case it is wpf testing or something else where
        //   stdout and debug out is not available
        public string dumpAsString()
        {
            return (HZincWriter.gridToString(this));
        }

        public IEnumerable<HCol> Cols => m_cols.AsEnumerable();

        public IEnumerable<HRow> Rows => m_rows.AsEnumerable();

        public IEnumerator<HRow> GetEnumerator() => m_rows.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => m_rows.GetEnumerator();
    }
}