//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.Collections.Generic;

namespace ProjectHaystack
{
    public class HRow : HDict
    {
        private List<HVal> m_cells;

        // internal constructor
        internal HRow(HGrid grid, List<HVal> cells) : base(new Dictionary<string, HVal>(11))
        {
            m_cells = cells;
            Grid = grid;
        }

        //////////////////////////////////////////////////////////////////////////
        // Access 
        //////////////////////////////////////////////////////////////////////////
        public HGrid Grid { get; }

        // Number of columns in the grid - even thought List<T> will allow
        //   Null values Grid constructor does a number of checks to ensure
        //   unique name and other checks particularly on Column - this means
        //   it assumes none are HCol null values (null values bypass the checking).  
        //   So the reality is null is not allowed in the code logic.
        //   Null is allowed in the haystack.org docs but I don't see an example
        //   and can't think of a practical example in it's use (think it was added
        //   just because it is a possible programming example).
        public override int Size
        {
            get { return Grid.numCols; }
        }

        // Get a cell by column name.  If the column is undefined or
        //    the cell is null then raise UnknownNameException or return
        //    null based on checked flag. 
        public override HVal get(string name, bool bchecked)
        {
            HCol col = Grid.col(name, false);
            if (col != null)
            {
                HVal val = get(col, bchecked);
                if (val != null) return val;
            }
            if (bchecked) throw new UnknownNameException(name);
            return null;
        }

        public override string getKeyAt(int iIndex, bool bChecked)
        {
            string strRet = null;

            if (iIndex < Size)
            {
                HCol col = Grid.col(iIndex);
                strRet = col.Name;
            }
            if ((strRet != null) || (!bChecked))
                return strRet;
            else
                throw new IndexOutOfRangeException(iIndex.ToString() + " out of range");
        }

        // Get a cell by column.  If cell is null then raise
        //    UnknownNameException or return  null based on checked flag. 
        public HVal get(HCol col, bool bchecked)
        {
            HVal val = m_cells[col.Index];
            if (val != null) return val;
            // Ian Davies - .NET port - this could cause a unknown name exception
            //              for a null value - back to previous comment
            if (bchecked) throw new UnknownNameException(col.Name);
            return null;
        }

        /* Iterator - removed as it is not needed
        public IEnumerable<HRow> iterator
        {
            get { return m_grid.iterator(); }
        } */
    }
}
