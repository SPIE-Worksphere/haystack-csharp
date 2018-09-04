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
    public class HUnknownWatchException : Exception
    {
        private string m_strWatchid;

        public string WatchID { get { return m_strWatchid; } }
        public HUnknownWatchException(string id) : base(id + " watch not known")
        {
            m_strWatchid = id;
        }
        public HUnknownWatchException(string id, string message)
        : base(message)
        {
            m_strWatchid = id;
        }

        public HUnknownWatchException(string id, string message, Exception inner)
        : base(message, inner)
        {
            m_strWatchid = id;
        }
    }
}
