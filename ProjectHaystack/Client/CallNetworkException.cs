//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;

namespace ProjectHaystack.Client
{
  /// <summary>
  /// CallNetworkException is thrown by HClient when there is a network I/O
  /// or connection problem with communication to the server.
  /// </summary>
  public class CallNetworkException : CallException
  {

    /// <summary>
    /// Constructor with cause exception </summary>
    public CallNetworkException(Exception cause)
      : base(cause.ToString(), cause){}

  }
}