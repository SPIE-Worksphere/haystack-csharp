//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;

namespace ProjectHaystack.Auth
{
  using CallException = ProjectHaystack.Client.CallException;
  /// <summary>
  /// AuthException is thrown by the authentication framework if an error occurs while trying
  /// to authenticat a user.
  /// </summary>
  public class AuthException : CallException
  {
    public AuthException(string s)
      : base(s){}
    public AuthException(string s, Exception throwable)
      : base(s, throwable){}
  }

}