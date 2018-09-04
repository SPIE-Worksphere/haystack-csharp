//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//   08 Aug 2018 Ian Davies Chenged to Grid to Align to Java Toolkit
//

using System.Collections.Generic;

namespace ProjectHaystack.Client
{
    /// <summary>
    /// CallErrException is thrown then a HClient.call returns a
    /// HGrid with the err marker tag indicating a server side error.
    /// </summary>
    public class CallErrException : CallException
    {
        // Error grid returned by server 
        private HGrid m_grid;

        // Access
        public HGrid Grid { get { return m_grid; } }

        // Constructor with error grid 
        public CallErrException(HGrid grid)
            : base(Msg(grid))
        {
            m_grid = grid;
        }

        private static string Msg(HGrid grid)
        {
            HVal dis = grid.meta.get("dis", false);
            if (dis is HStr) return ((HStr)dis).Value;
            return "server side error";
        }

        /** Get the server side stack trace or return null if not available */
        public string trace()
        {
            HVal val = m_grid.meta.get("errTrace", false);
            if (val is HStr) return ((HStr)val).ToString();
            return null;
        }

    }
}