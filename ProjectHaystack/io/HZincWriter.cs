//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   16 July 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.io
{
    /**
     * HZincWriter is used to write grids in the Zinc format
     *
     * @see <a href='http://project-haystack.org/doc/Zinc'>Project Haystack</a>
     */
    public class HZincWriter : HGridWriter
    {
        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////

        // Version of Zinc to write 
        private int m_iVersion;

        private StreamWriter m_swOut;
        private int m_iGridDepth;

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////
        public override Stream BaseStream { get { return m_swOut.BaseStream; } }

        //////////////////////////////////////////////////////////////////////////
        // Utils
        //////////////////////////////////////////////////////////////////////////
        // Write a grid to an in-memory a string 
        public static string gridToString(HGrid grid)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter swOut = new StreamWriter(ms);
            new HZincWriter(swOut).writeGrid(grid);

            StreamReader sr = new StreamReader(ms);
            ms.Position = 0;
            return sr.ReadToEnd();
        }
        public static string valToString(HVal val)
        {
            MemoryStream msOut = new MemoryStream();
            StreamWriter swOut = new StreamWriter(msOut);
            new HZincWriter(swOut).writeVal(val);
            StreamReader sr = new StreamReader(msOut);
            msOut.Position = 0;
            return sr.ReadToEnd();
        }



        //////////////////////////////////////////////////////////////////////////
        // Construction
        //////////////////////////////////////////////////////////////////////////

        // Write using UTF-8 
        public HZincWriter(StreamWriter swOut)
        {
            m_swOut = new StreamWriter(swOut.BaseStream, Encoding.UTF8);
            // IOException in .NET is not possible with this constructor - No need to catch and bubble just don't catch.
            m_iGridDepth = 0;
            m_iVersion = 3;
        }

        public HZincWriter (Stream strmOut)
        {
            m_swOut = new StreamWriter(strmOut, Encoding.UTF8);
            // IOException in .NET is not possible with this constructor - No need to catch and bubble just don't catch.
            m_iGridDepth = 0;
            m_iVersion = 3;
        }
        // Flush underlying output stream 
        public override void flush()
        {
            m_swOut.Flush();
        }

        // Close underlying output stream 
        public override void close()
        {
            m_swOut.Close();
        }

        // Write a zinc value 
        public HZincWriter writeVal(HVal val)
        {
            if (val is HGrid)
            {
                HGrid grid = (HGrid)val;
                try
                {
                    bool insideGrid = m_iGridDepth > 0;
                    ++m_iGridDepth;
                    if (insideGrid)
                        writeNestedGrid(grid);
                    else
                        writeGrid(grid);
                }
                finally
                {
                    --m_iGridDepth;
                }
            }
            else if (val is HList) writeList((HList)val);
            else if (val is HDict) writeDict((HDict)val);
            else writeScalar(val);
            return this;
        }

        private void writeNestedGrid(HGrid grid)
        {
            p("<<").nl();
            writeGrid(grid);
            p(">>");
            flush();
        }

        private void writeList(HList list)
        {
            p('[');
            for (int i = 0; i < list.size(); ++i)
            {
                if (i > 0)
                    p(',');
                writeVal(list.get(i));
            }
            p(']');
            flush();
        }

        private void writeDict(HDict dict)
        {
            p('{').writeDictKeyVals(dict).p('}');
            flush();
        }

        private void writeScalar(HVal val)
        {
            if (val == null) p('N');
            else if (val is HBin)
                writeBin((HBin)val);
            else if (val is HXStr)
                writeXStr((HXStr)val);
            else
                p(val.toZinc());
            flush();
        }


        private void writeBin(HBin bin)
        {
            if (m_iVersion < 3)
            {
                p("Bin(").p(bin.mime).p(')');
            }
            else
            {
                p(bin.toZinc());
                p("Bin(").p('"').p(bin.mime).p('"').p(')');
            }
            flush();
        }

        private void writeXStr(HXStr xstr)
        {
            if (m_iVersion < 3) throw new Exception("XStr not supported for version: " + m_iVersion.ToString());
            p(xstr.toZinc());
            flush();
        }

        //////////////////////////////////////////////////////////////////////////
        // HGridWriter
        //////////////////////////////////////////////////////////////////////////

        // Write a grid 
        public override void writeGrid(HGrid grid)
        {
            // meta
            p("ver:\"").p(m_iVersion).p(".0\"").writeMeta(grid.meta).nl();

            // cols
            if (grid.numCols == 0)
            {
                // technically this shoudl be illegal, but
                // for robustness handle it here
                throw new ArgumentException("Grid has no cols", "grid");
            }
            else
            {
                for (int i = 0; i < grid.numCols; ++i)
                {
                    if (i > 0) p(',');
                    writeCol(grid.col(i));
                }
            }
            nl();

            // rows
            for (int i = 0; i < grid.numRows; ++i)
            {
                writeRow(grid, grid.row(i));
                nl();
            }
            flush();
        }

        private HZincWriter writeMeta(HDict meta)
        {
            if (meta.isEmpty()) return this;
            p(' ');
            flush();
            return writeDictKeyVals(meta);
        }

        private HZincWriter writeDictKeyVals(HDict dict)
        {
            if (dict.isEmpty()) return this;
            bool bFirst = true;
            for (int i = 0; i < dict.size(); i++)
            {
                string name = dict.getKeyAt(i, false);
                if (name != null)
                {
                    HVal val = (HVal)dict.get(name, false);
                    if (!bFirst) p(' ');
                    p(name);
                    if (val != HMarker.VAL)
                    {
                        p(':').writeVal(val);
                    }
                    bFirst = false;
                }
            }
            flush();
            return this;
        }

        private void writeCol(HCol col)
        {
            p(col.Name).writeMeta(col.meta);
            flush();
        }

        private void writeRow(HGrid grid, HRow row)
        {
            for (int i = 0; i < grid.numCols; ++i)
            {
                HVal val = row.get(grid.col(i), false);
                if (i > 0)
                    m_swOut.Write(',');
                if (val == null)
                {
                    if (i == 0)
                        m_swOut.Write('N');
                }
                else
                {
                    m_swOut.Write(val.toZinc());
                }
            }
            flush();
        }

        //////////////////////////////////////////////////////////////////////////
        // Member Utils - These were print - changed to write
        //////////////////////////////////////////////////////////////////////////

        private HZincWriter p(int i)
        {
            m_swOut.Write(i);
            return this;
        }
        private HZincWriter p(char c)
        {
            m_swOut.Write(c);
            return this;
        }
        private HZincWriter p(object obj)
        {
            m_swOut.Write(obj);
            return this;
        }
        private HZincWriter nl()
        {
            m_swOut.Write('\n');
            return this;
        }

    }
}
