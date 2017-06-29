//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

namespace ProjectHaystack.Client
{
  /// <summary>
  /// CallHttpException is thrown by HClient when communication
  /// is successful with a server, but we receive an error HTTP
  /// error response.
  /// </summary>
  public class CallHttpException : CallException
  {

    /// <summary>
    /// Constructor with code such as 404 and response message </summary>
    public CallHttpException(int code, string msg)
      : base("" + code + ": " + msg)
    {
      this.code = code;
    }

    /// <summary>
    /// Response code such as 404 </summary>
    public readonly int code;

  }
}