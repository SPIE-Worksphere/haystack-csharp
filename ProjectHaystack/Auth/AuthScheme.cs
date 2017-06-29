//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace ProjectHaystack.Auth
{

  /// <summary>
  /// AuthScheme is the base class for modeling pluggable authentication algorithms.
  /// </summary>
  public abstract class AuthScheme
  {

    //////////////////////////////////////////////////////////////////////////
    // Registry
    //////////////////////////////////////////////////////////////////////////

    public static AuthScheme Find(string name)
    {
      return AuthScheme.Find(name, true);
    }

    /// <summary>
    /// Lookup an AuthScheme for the given case-insensitive name.
    /// </summary>
    public static AuthScheme Find(string name, bool @checked)
    {
      AuthScheme scheme = (AuthScheme)registry[name];
      if (scheme != null)
      {
        return scheme;
      }
      if (@checked)
      {
        throw new System.ArgumentException("No auth scheme found for '" + name + "'");
      }
      return null;
    }

    public static AuthScheme[] List()
    {
      AuthScheme[] tmp = new AuthScheme[registry.Count];
      IList<AuthScheme> tmpValues = registry.Values.ToList();
      for (int i = 0; i < registry.Count; i++)
      {
        tmp[i] = tmpValues[i];
      }
      return tmp;
    }

    private static SortedDictionary<string, AuthScheme> registry = new SortedDictionary<string, AuthScheme>();
    static AuthScheme()
    {
      registry["scram"] = new ScramScheme();
      registry["basic"] = new BasicScheme();
    }

    //////////////////////////////////////////////////////////////////////////
    // Construction
    //////////////////////////////////////////////////////////////////////////

    protected internal AuthScheme(string name)
    {
      if (name != name.ToLower())
      {
        throw new System.ArgumentException("Name must be lowercase: " + name);
      }
      this.name = name;
    }

    //////////////////////////////////////////////////////////////////////////
    // Overrides
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Scheme name (always normalized to lowercase)
    /// </summary>
    public readonly string name;

    /// <summary>
    ///	Handle a standardized client authentication challenge message from
    ///	the server using RFC 7235.
    /// </summary>
    /// <param name="cx">the current AuthClientContext</param>
    /// <param name="msg">the AuthMsg sent by the server</param>
    /// <returns>AuthMsg to send back to the server to authenticate</returns>
    public abstract AuthMsg OnClient(AuthClientContext cx, AuthMsg msg);

    /// <summary>
    ///	Callback after successful authentication with the server.
    ///	The default implementation is a no-op.
    /// </summary>
    /// <param name="cx">the current AuthClientContext</param>
    /// <param name="msg">AuthMsg sent by the server when it authenticated the client</param>
    public virtual void OnClientSuccess(AuthClientContext cx, AuthMsg msg)
    {
    }


    /// <summary>
    ///	Handle non-standardized client authentication when the standard
    ///	process (RFC 7235) fails. If this scheme thinks it can handle the
    ///	given response by sniffing the response code and headers, then it
    ///	should process and return true.
    /// </summary>
    /// <param name="cx">the current AuthClientContext</param>
    /// <param name="resp">the response message from the server</param>
    /// <param name="content">the body of the response if it had one, or null</param>
    /// <returns>true if the scheme processed the response, false otherwise. Returns false by default</returns>
    public virtual bool OnClientNonStd(AuthClientContext cx, HttpWebResponse resp, string content)
    {
      return false;
    }
  }

}