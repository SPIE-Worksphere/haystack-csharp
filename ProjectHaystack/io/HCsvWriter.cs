//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
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
     * HCsvWriter is used to write grids in comma separated values
     * format as specified by RFC 4180.  Format details:
     * <ul>
     * <li>rows are delimited by a newline</li>
     * <li>cells are separated by configured delimiter char (default is comma)</li>
     * <li>cells containing the delimiter, '"' double quote, or
     *     newline are quoted; quotes are escaped as with two quotes</li>
     * </ul>
     *
     * @see <a href='http://project-haystack.org/doc/Csv'>Project Haystack</a>
     */
    public class HCsvWriter : HGridWriter
    {
        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////

        // Delimiter used to write each cell 
        private char m_cDelimiter;

        private StreamWriter m_swOut;

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////
        public override Stream BaseStream { get { return m_swOut.BaseStream; } }

        //////////////////////////////////////////////////////////////////////////
        // Construction
        //////////////////////////////////////////////////////////////////////////

        // Write using UTF-8 
        public HCsvWriter(Stream strmOut, bool encoderShouldEmitUTF8Identifier = false)
        {
            m_cDelimiter = ',';
            m_swOut = new StreamWriter(strmOut, new UTF8Encoding(encoderShouldEmitUTF8Identifier));
            // Java implementation bubbled exception - don't see it adds value
        }

        //////////////////////////////////////////////////////////////////////////
        // HGridWriter
        //////////////////////////////////////////////////////////////////////////

        // Write a grid 
        public override void writeGrid(HGrid grid)
        {
            // cols
            for (int i = 0; i < grid.numCols; ++i)
            {
                if (i > 0)
                    m_swOut.Write(m_cDelimiter);
                writeCell(grid.col(i).dis());
            }
            m_swOut.Write('\n');

            // rows
            for (int i = 0; i < grid.numRows; ++i)
            {
                writeRow(grid, grid.row(i));
                m_swOut.Write('\n');
            }
        }

        private void writeRow(HGrid grid, HRow row)
        {
            for (int i = 0; i < grid.numCols; ++i)
            {
                HVal val = row.get(grid.col(i), false);
                if (i > 0)
                    m_swOut.Write(m_cDelimiter);
                writeCell(valToString(val));
            }
        }

        private string valToString(HVal val)
        {
            if (val == null) return "";

            if (val == HMarker.VAL) return "\u2713";

            if (val is HRef)
            {
                HRef href = (HRef)val;
                string s = "@" + href.val;
                if (href.disSet) s += " " + href.display();
                return s;
            }

            return val.ToString();
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

        //////////////////////////////////////////////////////////////////////////
        // CSV
        //////////////////////////////////////////////////////////////////////////

        // Write a cell 
        public void writeCell(string cell)
        {
            if (!isQuoteRequired(cell))
            {
                m_swOut.Write(cell);
            }
            else
            {
                m_swOut.Write('"');
                for (int i = 0; i < cell.Length; ++i)
                {
                    int c = cell[i];
                    if (c == '"')
                        m_swOut.Write('"');
                    m_swOut.Write((char)c);
                }
                m_swOut.Write('"');
            }
        }

        /**
         * Return if the given cell string contains:
         * <ul>
         * <li>the configured delimiter</li>
         * <li>double quote '"' char</li>
         * <li>leading/trailing whitespace</li>
         * <li>newlines</li>
         * </ul>
         */
        public bool isQuoteRequired(string cell)
        {
            if (cell.Length == 0) return true;
            if (isWhiteSpace(cell[0])) return true;
            if (isWhiteSpace(cell[cell.Length - 1])) return true;
            for (int i = 0; i < cell.Length; ++i)
            {
                int c = cell[i];
                if (c == m_cDelimiter || c == '"' || c == '\n' || c == '\r')
                    return true;
            }
            return false;
        }

        static bool isWhiteSpace(int c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

    }
}
