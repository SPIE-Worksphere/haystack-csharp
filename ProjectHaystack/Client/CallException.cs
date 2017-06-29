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
  /// CallException base class for exceptions thrown HClient.call.
  /// Subclasses:
  /// <ul>
  /// <li>CallNetworkException: network communication error</li>
  /// <li>CallHttpException: HTTP response error such as 404</li>
  /// <li>CallErrException: server errors with server side stack Trace</li>
  /// <li>CallAuthException: authentication error</li>
  /// </ul>
  /// </summary>
  public class CallException : Exception
  {

    /// <summary>
    /// Constructor with message </summary>
    public CallException(string msg)
      : base(msg){}

    /// <summary>
    /// Constructor with message and cause </summary>
    public CallException(string msg, Exception cause)
      : base(msg, cause){}

  }
}