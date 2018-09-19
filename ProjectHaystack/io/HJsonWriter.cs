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
     * HJsonWriter is used to write grids in JavaScript Object Notation.
     * It is a plain text format commonly used for serialization of data.
     ** It is specified in RFC 4627.
     *
     * @see <a href='http://project-haystack.org/doc/Json'>Project Haystack</a>
     */
    public class HJsonWriter : HGridWriter
    {
        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////

        private StreamWriter m_swOut;

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
            new HJsonWriter(swOut).writeGrid(grid);
            StreamReader sr = new StreamReader(ms);
            ms.Position = 0;
            return sr.ReadToEnd();
        }

        public static string valToString(HVal val)
        {
            MemoryStream msOut = new MemoryStream();
            StreamWriter swOut = new StreamWriter(msOut);
            new HJsonWriter(swOut).writeVal(val);
            StreamReader sr = new StreamReader(msOut);
            msOut.Position = 0;
            return sr.ReadToEnd();
        }

        //////////////////////////////////////////////////////////////////////////
        // Construction
        //////////////////////////////////////////////////////////////////////////

        // Write using UTF-8 
        public HJsonWriter(StreamWriter swOut)
        {
            m_swOut = new StreamWriter(swOut.BaseStream, Encoding.UTF8);
            // IOException in .NET is not possible with this constructor - No need to catch and bubble just don't catch.
        }

        // Constructor with stringwriter replaced with memory steam (has a Tostring function)
        private HJsonWriter(MemoryStream msOut)
        {
            m_swOut = new StreamWriter(msOut, Encoding.UTF8);
            // IOException in .NET is not possible with this constructor - No need to catch and bubble just don't catch.
        }

        public HJsonWriter(Stream strmOut)
        {
            m_swOut = new StreamWriter(strmOut, Encoding.UTF8);
            // IOException in .NET is not possible with this constructor - No need to catch and bubble just don't catch.
        }

        //////////////////////////////////////////////////////////////////////////
        // HGridWriter
        //////////////////////////////////////////////////////////////////////////

        // Write a grid 
        public override void writeGrid(HGrid grid)
        {
            // grid begin
            m_swOut.Write("{\n");

            // meta
            m_swOut.Write("\"meta\": {\"ver\":\"2.0\"");
            writeDictTags(grid.meta, false);
            m_swOut.Write("},\n");

            // columns
            m_swOut.Write("\"cols\":[\n");
            for (int i = 0; i < grid.numCols; i++)
            {
                if (i > 0)
                    m_swOut.Write(",\n");
                HCol col = grid.col(i);
                m_swOut.Write("{\"name\":");
                m_swOut.Write(HStr.toCode(col.Name)); 
                writeDictTags(col.meta, false);
                m_swOut.Write("}");
            }
            m_swOut.Write("\n],\n");

            // rows
            m_swOut.Write("\"rows\":[\n");
            for (int i = 0; i < grid.numRows; i++)
            {
                if (i > 0)
                    m_swOut.Write(",\n");
                writeDict(grid.row(i));
            }
            m_swOut.Write("\n]\n");

            // grid end
            m_swOut.Write("}\n");
            m_swOut.Flush();
        }

        private void writeDict(HDict dict)
        {
            m_swOut.Write("{");
            writeDictTags(dict, true); 
            
            m_swOut.Write("}");
        }

        private void writeDictTags(HDict dict, bool first)
        {
            bool bLocalFirst = first;
            // Is ther a multi-threaded issue here - for a changing size of dict
            for (int i = 0; i < dict.size(); i++)
            {
                if (bLocalFirst)
                    bLocalFirst = false;
                else
                    m_swOut.Write(", ");
                string name = dict.getKeyAt(i, true);
                HVal val = dict.get(name, false); 
                m_swOut.Write(HStr.toCode(name));
                m_swOut.Write(":");
                writeVal(val);
            }
        }

        private void writeVal(HVal val)
        {
            if (val == null)
                m_swOut.Write("null");
            else if (val is HBool)
                m_swOut.Write(val);
            else if (val is HDict)
                writeDict((HDict)val);
            else if (val is HGrid)
                writeGrid((HGrid)val);
            else
                m_swOut.Write(HStr.toCode(val.toJson()));
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

    }
}
