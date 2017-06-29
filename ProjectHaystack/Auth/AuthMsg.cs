//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ProjectHaystack.Auth
{
  using WebUtil = ProjectHaystack.Util.WebUtil;

  /// <summary>
  /// AuthMsg models a scheme name and set of parameters according
  /// to <a href="https://tools.ietf.org/html/rfc7235">RFC 7235</a>. To simplify
  /// parsing, we restrict the grammar to be auth-param and token (the
  /// token68 and quoted-string productions are not allowed).
  /// </summary>
  public sealed class AuthMsg
  {

    //////////////////////////////////////////////////////////////////////////
    // Construction
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Parse a List of AuthSchemes such as a List of 'challenge'
    /// productions for the WWW-Authentication header per RFC 7235.
    /// </summary>
    public static AuthMsg[] ListFromStr(string s)
    {
      string[] toks = SplitList(s);
      AuthMsg[] arr = new AuthMsg[toks.Length];
      for (int i = 0; i < toks.Length; ++i)
      {
        arr[i] = AuthMsg.FromStr(toks[i]);
      }
      return arr;
    }

    public static AuthMsg FromStr(string s)
    {
      return FromStr(s, true);
    }
    public static AuthMsg FromStr(string s, bool @checked)
    {
      try
      {
        return Decode(s);

      }
      catch (Exception e)
      {
        if (@checked)
        {
          throw new FormatException(e.ToString());
        }
        return null;
      }
    }

    public AuthMsg(string scheme, IDictionary<string, string> @params)
    {
      this.scheme = scheme.ToLower();
      this.@params = new SortedDictionary<string, string>(@params, StringComparer.OrdinalIgnoreCase);
      this.toStr = Encode(this.scheme, this.@params);
    }

    public AuthMsg(string scheme, string[] @params)
      : this(scheme, ListToParams(@params))
    {
    }



    private static SortedDictionary<string, string> CaseInsensitiveMap(IDictionary @params)
    {
      SortedDictionary<string, string> treeMap = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      IEnumerator iter = @params.Keys.GetEnumerator();
      while (iter.MoveNext())
      {
        string key = (string)iter.Current;
        string val = (string)@params[key];
        treeMap[key.ToLower()] = val;
      }
      return treeMap;
    }

    private static SortedDictionary<string, string> ListToParams(string[] @params)
    {
      if (@params.Length % 2 != 0)
      {
        throw new System.ArgumentException("odd number of params");
      }
      SortedDictionary<string, string> map = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      for (int i = 0; i < @params.Length; i = i + 2)
      {
        map[@params[i]] = @params[i + 1];
      }
      return map;
    }

    //////////////////////////////////////////////////////////////////////////
    // Identity
    //////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Scheme name normalized to lowercase </summary>
    public readonly string scheme;

    /// <summary>
    /// Parameters for scheme (read-only) </summary>
    internal readonly IDictionary @params;

    private readonly string toStr;

    public override bool Equals(object o)
    {
      if (this == o)
      {
        return true;
      }
      if (o == null || this.GetType() != o.GetType())
      {
        return false;
      }

      AuthMsg authMsg = (AuthMsg)o;
      if (!scheme.Equals(authMsg.scheme))
      {
        return false;
      }
      var comparer = new SortedDictComparer();
      return comparer.Compare(@params, authMsg.@params) == 1;

    }

    public override int GetHashCode()
    {
      int result = scheme.GetHashCode();
      IEnumerator iter = @params.Keys.GetEnumerator();
      while (iter.MoveNext())
      {
        string key = (string)iter.Current;
        result = 31 * result + key.ToLower().GetHashCode();
        result = 31 * result + @params[key].GetHashCode();
      }
      return result;
    }

    public override string ToString()
    {
      return toStr;
    }

    /// <summary>
    /// Convenience for <seealso cref="#param(String, boolean) param(name, true)"/>
    /// </summary>
    public string Param(string name)
    {
      return Param(name, true);
    }

    /// <summary>
    /// Lookup a parameter by name. If not found and checked,
    /// throw an Exception, otherwise return null.
    /// </summary>
    public string Param(string name, bool @checked)
    {
      string val = (string)@params[name];
      if (!string.ReferenceEquals(val, null))
      {
        return val;
      }
      if (@checked)
      {
        throw new Exception("parameter not found: " + name);
      }
      return null;
    }

    //////////////////////////////////////////////////////////////////////////
    // Encoding
    //////////////////////////////////////////////////////////////////////////

    public static string[] SplitList(string s)
    {
      // Find break indices (start of each challenge production)
      string[] toks = s.Split(',');
      for (int i = 0; i < toks.Length; ++i)
      {
        toks[i] = toks[i].Trim();
      }

      var breaks = new ArrayList();
      for (int i = 0; i < toks.Length; ++i)
      {
        string tok = toks[i];
        int sp = tok.IndexOf(' ');
        string name = (sp < 0 ? tok : tok.Substring(0, sp)).Trim();
        if (WebUtil.IsToken(name) && i > 0)
        {
          breaks.Add((int)i);
        }
      }
      // rejoin tokens into challenge strings
      string[] acc = new string[toks.Length];
      int start = 0;
      int splitCounter = 0;
      IEnumerator iter = breaks.GetEnumerator();
      StringBuilder sb = new StringBuilder();
      while (iter.MoveNext())
      {
        sb.Length = 0;
        int end = ((int?)iter.Current).Value;
        for (int i = start; i < end; ++i)
        {
          if (i > start)
          {
            sb.Append(",");
          }
          sb.Append(toks[i]);
        }
        acc[splitCounter] = (sb.ToString());
        splitCounter += 1;
        start = end;
      }
      sb.Length = 0;
      for (int i = start; i < toks.Length; ++i)
      {
        if (i > start)
        {
          sb.Append(",");
        }
        sb.Append(toks[i]);
      }
      acc[splitCounter] = (sb.ToString());
      acc = acc.Where(x => !string.IsNullOrEmpty(x)).ToArray();
      return acc;
    }

    private static AuthMsg Decode(string s)
    {
      int sp = s.IndexOf(' ');
      string scheme = s;
      SortedDictionary<string, string> @params = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      if (sp >= 0)
      {
        scheme = s.Substring(0, sp);
        string[] paramParts = s.Substring(sp + 1).Split(',');
        for (int i = 0; i < paramParts.Length; i++)
        {
          string part = paramParts[i].Trim();
          int eq = part.IndexOf('=');
          if (eq < 0)
          {
            throw new FormatException("Invalid auth-param: " + part);
          }
          @params[part.Substring(0, eq).Trim().ToLower()] = part.Substring(eq + 1).Trim();
        }
      }
      return new AuthMsg(scheme, @params);
    }

    public static string Encode(string scheme, IDictionary @params)
    {
      @params = CaseInsensitiveMap(@params);
      StringBuilder sb = new StringBuilder();
      AddToken(sb, scheme);
      IEnumerator iter = @params.Keys.GetEnumerator();
      bool first = true;
      while (iter.MoveNext())
      {
        string key = (string)iter.Current;
        key = key.ToLower();
        string val = (string)@params[key];
        if (first)
        {
          first = false;
        }
        else
        {
          sb.Append(',');
        }
        sb.Append(' ');
        AddToken(sb, key);
        sb.Append('=');
        AddToken(sb, val);
      }
      return sb.ToString();
    }

    private static void AddToken(StringBuilder sb, string val)
    {
      for (int i = 0; i < val.Length; ++i)
      {
        int cp = char.ConvertToUtf32(val, i);
        if (WebUtil.IsTokenChar(cp))
        {
          sb.Append(val[i]);
        }
        else
        {
          throw new Exception("Invalid char '" + val[i] + "' in " + val);
        }
      }
    }

  }

}