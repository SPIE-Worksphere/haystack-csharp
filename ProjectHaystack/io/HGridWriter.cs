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

namespace ProjectHaystack.io
{
    /**
     * HGridWriter is base class for writing grids to an output stream.
     *
     * @see <a href='http://project-haystack.org/doc/Rest#contentNegotiation'>Project Haystack</a>
     */
    public abstract class HGridWriter
    {

        // Write a grid 
        public abstract void writeGrid(HGrid grid);

        // Flush output stream 
        public abstract void flush();

        // Close output stream 
        public abstract void close();

        // Access to undlying stream
        public abstract System.IO.Stream BaseStream { get; }
    }
}
