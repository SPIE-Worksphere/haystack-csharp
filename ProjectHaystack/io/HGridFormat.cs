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
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProjectHaystack.io
{
    /**
     * HGridFormat models a format used to encode/decode HGrid.
     *
     * @see <a href='http://project-haystack.org/doc/Rest#contentNegotiation'>Project Haystack</a>
     */
    public class HGridFormat
    {
        private Dictionary<string, HGridFormat> m_registry; 
        private readonly object m_syncLock;
        /**
         * Mime type for the format with no paramters, such as "text/zinc".
         * All text formats are assumed to be utf-8.
         */
        private string m_strMime;

        /**
        * Class of HGridReader used to read this format
        * or null if reading is unavailable.
        */
        private HGridReader m_gridReader;

        /**
        * Class of HGridWriter used to write this format
        * or null if writing is unavailable.
        */
        private HGridWriter m_gridWriter;
        // Access
        public string Mime { get { return m_strMime; } }
        public HGridReader Reader { get { return m_gridReader; } }
        public HGridWriter Writer { get { return m_gridWriter; } }

        // Constructor 
        public HGridFormat(string mime, HGridReader reader, HGridWriter writer)
        {
            
            if (mime.IndexOf(';') >= 0)
                throw new ArgumentException("mime has semicolon " + mime, "mime");
            m_strMime = mime;
            m_gridReader = reader;
            m_gridWriter = writer;
            m_registry = new Dictionary<string, HGridFormat>();
            m_syncLock = new object();
            // The original implmentation had a storage of the generic type aginst the formats - but I see this 
            // as a distinct issue because:
            //  1. if this is not storing a real instance with underlying streams attempts to use it should cause
            //     issues unless makereader /writer is called with the instance - this makes the interface not well
            //     designed to oo principals and design patterns - implementation should not hide obvious pitfalls in 
            //     calling
            // Solution has been to get the underlying stream and construct a real instance
            register(new HGridFormat("text/plain", new HZincReader(reader.BaseStream), new HZincWriter(writer.BaseStream)));
            register(new HGridFormat("text/zinc", new HZincReader(reader.BaseStream), new HZincWriter(writer.BaseStream)));
            //register(new HGridFormat("text/csv",         null, HCsvWriter.class));
            register(new HGridFormat("application/json", null, new HJsonWriter(writer.BaseStream)));
        }


        // Make instance of "reader"; constructor with InputStream is expected.
        public HGridReader makeReader(StreamReader instm)
        {
            if (m_gridReader == null) throw new Exception("Format doesn't support reader: " + m_strMime);
            try
            {
                // For each possible case make reader of the correct type based on the stream
                if (m_gridReader is HZincReader) m_gridReader = new HZincReader(instm.BaseStream);
                return m_gridReader;
            }
            catch (Exception e)
            {
                throw new Exception("Cannot construct: " + m_gridReader.GetType() + "(InputStream)", e);
            }
        }

        // Make instance of "writer"; constructor with OutputStream is expected.
        public HGridWriter makeWriter(StreamWriter outstm)
        {
            if (m_gridWriter == null)
                throw new Exception("Format doesn't support writer: " + m_strMime);
            try
            {
                if (m_gridWriter is HZincWriter) m_gridWriter = new HZincWriter(outstm);
                else if (m_gridWriter is HJsonWriter) m_gridWriter = new HJsonWriter(outstm);
                return m_gridWriter;
            }
            catch (Exception e)
            {
                throw new Exception("Cannot construct: " + m_gridWriter.GetType() + "(OutputStream)", e);
            }
        }

        /**
         * Find the HGridFormat for the given mime type.  The mime type
         * may contain parameters in which case they are automatically stripped
         * for lookup.  Throw a RuntimeException or return null based on
         * checked flag if the mime type is not registered to a format.
         */
        // In Java this was static but that is illegal to access instance members
        public HGridFormat find(string mime, bool bChecked)
        {
            // normalize mime type to strip parameters
            int semicolon = mime.IndexOf(';');
            if (semicolon > 0)
                mime = mime.Substring(0, semicolon).Trim();

            // lookup format
            HGridFormat format = null;
            lock(m_syncLock)
            {
                format = m_registry[mime];
            }
            if (format != null)
                return format;
            else
            {
                if (bChecked)
                    throw new Exception("No format for mime type: " + mime);
                return null;
            }           
            
        }

        // List all registered formats
        public HGridFormat[] list()
        {
            HGridFormat[] acc = null;
            lock (m_syncLock)
            {
                acc = new HGridFormat[m_registry.Count];
                m_registry.Values.CopyTo(acc, 0);
            }
            return acc;
        }

        // Register a new HGridFormat
        public void register(HGridFormat format)
        {
            lock (m_syncLock)
            {
                m_registry.Add(format.Mime, format);
            }
        }
    }
}
