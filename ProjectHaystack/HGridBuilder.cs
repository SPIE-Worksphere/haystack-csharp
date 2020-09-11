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

namespace ProjectHaystack
{
    //////////////////////////////////////////////////////////////////////////
    // BCol - Represents a Basic Column
    //////////////////////////////////////////////////////////////////////////

    internal class BCol
    {
        // Access 
        public HDictBuilder Meta { get; }
        public string Name { get; }

        public BCol(string name)
        {
            Name = name;
            Meta = new HDictBuilder();
        }
    }

    /**
     * HGridBuilder is used to construct an HGrid instance.
     *
     * @see <a href='http://project-haystack.org/doc/Grids'>Project Haystack</a>
     */
    public class HGridBuilder
    {
        private List<BCol> m_cols;
        private List<List<HVal>> m_rows;

        // Access
        // Important Note on Meta: This grid builder does not fill in any meta data (it 
        //   doesn't have it) access is granted to the instance for external to the grid builder 
        //   for the meta tags to be added externally - Intance/todict is passed to built grids meta
        public HDictBuilder Meta { get; }
        public int colCount { get { return m_cols.Count; } }
        public int rowCount { get { return m_rows.Count; } }
        internal BCol GetColAt(int iIndex)
        {
            if (iIndex >= m_cols.Count)
                return null;
            else return m_cols[iIndex];
        }
        //////////////////////////////////////////////////////////////////////////
        // Constructor
        //////////////////////////////////////////////////////////////////////////
        public HGridBuilder()
        {
            Meta = new HDictBuilder();
            m_cols = new List<BCol>();
            m_rows = new List<List<HVal>>();
        }

        //////////////////////////////////////////////////////////////////////////
        // Utils
        //////////////////////////////////////////////////////////////////////////

        // Convenience to build one row grid from HDict. 
        public static HGrid dictToGrid(HDict dict)
        {
            HGridBuilder b = new HGridBuilder();
            int iIndex = 0;
            List<HVal> cells = new List<HVal>();
            for (iIndex = 0; iIndex < dict.size(); iIndex++)
            {
                string strKey = dict.getKeyAt(iIndex, false);
                if (strKey != null)
                {
                    HVal val = dict.get(strKey);
                    b.addCol(strKey);
                    cells.Add(val);
                }
            }
            b.addRow(cells.ToArray());
            return b.toGrid();
        }
        // Convenience to build grid from array of HDict.
        //    Any null entry will be row of all null cells. 
        public static HGrid dictsToGrid(HDict[] dicts)
        {
            return dictsToGrid(HDict.Empty, dicts);
        }

        // Convenience to build grid from array of HDict.
        //    Any null entry will be row of all null cells. 
        public static HGrid dictsToGrid(HDict meta, HDict[] dicts)
        {
            HCol colEmpty = new HCol(0, "empty", HDict.Empty);
            // If empty return an empty cols collection and no values Grid
            if (dicts.Length == 0)
            {
                List<List<HVal>> rowListEmpty = new List<List<HVal>>();
                List<HCol> rowEmpty = new List<HCol>();
                rowEmpty.Add(colEmpty);
                return new HGrid(meta, rowEmpty, rowListEmpty);
            }

            HGridBuilder b = new HGridBuilder();
            b.Meta.add(meta);

            // collect column names - why does this need to be a dictionary (hashmap in the java code)?  
            //      it only stores the col name twice.
            Dictionary<string, string> colsByName = new Dictionary<string, string>();
            for (int i = 0; i < dicts.Length; ++i)
            {
                HDict dict = dicts[i];
                if (dict != null)
                {
                    for (int it = 0; it < dict.size(); it++)
                    {
                        string name = dict.getKeyAt(it, false);
                        if (name != null)
                        {
                            if (!colsByName.Keys.Contains(name))
                            {
                                colsByName.Add(name, name);
                                b.addCol(name);
                            }
                        }
                    }
                }
            }

            // if all dicts were null, handle special case
            // by creating a dummy column
            if (colsByName.Count == 0)
            {
                colsByName.Add("empty", "empty");
                b.addCol("empty");
            }

            // now map rows
            int numCols = b.colCount;
            for (int ri = 0; ri < dicts.Length; ++ri)
            {
                HDict dict = dicts[ri];
                HVal[] cells = new HVal[numCols];
                for (int ci = 0; ci < numCols; ++ci)
                {
                    if (dict == null)
                        cells[ci] = null;
                    else
                    {
                        BCol colatci = b.GetColAt(ci);
                        if (colatci != null)
                            cells[ci] = dict.get(colatci.Name, false);
                    }
                }
                b.addRow(cells);
            }

            return b.toGrid();
        }

        // Convenience to build an error grid from exception
        //   Replace Tabs with 2 spaces
        //   Filter out /r/n 
        //   YET TO DO - Test this - Adding is a bit strange
        // project-haystack:
        /*
         * Error Grid
         * If a operation failed after a request grid is successfully read by a server, 
         * then the server returns an error grid. An error grid is indicated by the presence of the err marker tag in the grid metadata. 
         * All error grids must also include a dis tag in the grid metadata with a human readable descripton of the problem. 
         * If the server runtime supports stack traces, this should be reported as a multi-line string via the errTrace tag in the grid metadata.
         * 
         * Example of an error grid encoded as Zinc:
         * ver:"3.0" err dis:"Cannot resolve id: badId" errTrace:"UnknownRecErr: badId\n  ...."
         * empty
         */
        public static HGrid errToGrid(Exception e)
        {
            // Java sucks - replaed with .NET
            StringBuilder sout = new StringBuilder();
            string trace = e.StackTrace.ToString();
            for (int i = 0; i < trace.Length; ++i)
            {
                int ch = trace[i];
                if (ch == '\t') sout.Append("  ");
                else if (ch != '\r') sout.Append((char)ch);
            }

            HGridBuilder b = new HGridBuilder();
            b.Meta.add("err")
                    .add("dis", e.Message)
                    .add("errTrace", sout.ToString());
            b.addCol("empty");
            return b.toGrid();
        }

        // Convenience to build grid from array of HHisItem 
        public static HGrid hisItemsToGrid(HDict meta, HHisItem[] items)
        {
            HGridBuilder b = new HGridBuilder();
            b.Meta.add(meta);
            b.addCol("ts");
            b.addCol("val");
            for (int i = 0; i < items.Length; ++i)
            {
                b.addRow(new HVal[] { items[i].TimeStamp, items[i].hsVal });
            }
            return b.toGrid();
        }

        //////////////////////////////////////////////////////////////////////////
        // Building
        //////////////////////////////////////////////////////////////////////////

        // Add new column and return builder for column metadata.
        //    Columns cannot be added after adding the first row. 
        public HDictBuilder addCol(string name)
        {
            if (m_rows.Count > 0)
                throw new InvalidOperationException("Cannot add cols after rows have been added");
            if (!HDict.isTagName(name))
                throw new ArgumentException("Invalid column name: " + name, "name");
            BCol col = new BCol(name);
            m_cols.Add(col);
            return col.Meta;
        }

        // Add new row with array of cells which correspond to column
        //    order.  Return this. 
        public HGridBuilder addRow(params HVal[] cells)
        {
            if (m_cols.Count() != cells.Length)
                throw new InvalidOperationException("Row cells size != cols size");
            List<HVal> vals = new List<HVal>(cells);
            m_rows.Add(vals);
            return this;
        }

        // Convert current state to an immutable HGrid instance 
        public HGrid toGrid()
        {
            // meta
            HDict meta = Meta.toDict();

            // cols
            List<HCol> hcols = new List<HCol>(m_cols.Count);
            for (int i = 0; i < m_cols.Count; ++i)
            {
                BCol bc = (BCol)m_cols[i];
                hcols.Add(new HCol(i, bc.Name, bc.Meta.toDict()));
            }

            // let HGrid constructor do the rest...
            return new HGrid(meta, hcols, m_rows);
        }
    }
}