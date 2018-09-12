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
    /**
     * UnknownNameException is thrown when attempting to perform
     * a checked lookup by name for a tag/col not present.
     */
    public class UnknownNameException : Exception
    {
        public UnknownNameException(string message) : base(message)
        {
        }
    }
}
