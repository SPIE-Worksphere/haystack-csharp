//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
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

    /// <summary>
    /// Constructor with error grid </summary>
    public CallErrException(Dictionary<string,string> grid)
      : base(Msg(grid))
    {
      this.grid = grid;
    }

    private static string Msg(Dictionary<string,string> grid)
    {
      var dis = grid["dis"];
      if (dis is string)
      {
        return dis;
      }
      return "server side error";
    }

    /// <summary>
    /// Error grid returned by server </summary>
    public readonly Dictionary<string,string> grid;

    /// <summary>
    /// Get the server side stack Trace or return null if not available </summary>
    public virtual string Trace()
    {
      var val = grid["errTrace"];
      if (val is string)
      {
        return val;
      }
      return null;
    }

  }
}