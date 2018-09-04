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
     * HGridReader is base class for reading grids from an input stream.
     *
     * @see <a href='http://project-haystack.org/doc/Rest#contentNegotiation'>Project Haystack</a>
     */
    // For pure abstract I would normally use Interface instead however in this case we will
    //  attempt to keep it as close to the original toolkit as possible
    public abstract class HGridReader
    {
        // Read a grid 
        public abstract HGrid readGrid();

        // Access to undlying stream
        public abstract System.IO.Stream BaseStream { get; }

    }
}
