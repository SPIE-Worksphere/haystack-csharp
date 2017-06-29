//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

namespace ProjectHaystack.Util
{
  public class WebUtil
  {
    public static bool IsToken(string s)
    {
      if (string.ReferenceEquals(s, null) || s.Length == 0)
      {
        return false;
      }
      for (int i = 0; i < s.Length; i++)
      {
        if (!IsTokenChar(char.ConvertToUtf32(s, i)))
        {
          return false;
        }
      }
      return true;

    }
    public static bool IsTokenChar(int codePoint)
    {
      return codePoint < 127 && tokenChars[codePoint];
    }

    private static bool[] tokenChars;
    static WebUtil()
    {
      bool[] m = new bool[127];
      for (int i = 0; i < 127; ++i)
      {
        m[i] = i > 0x20;
      }
      m['('] = false;
      m[')'] = false;
      m['<'] = false;
      m['>'] = false;
      m['@'] = false;
      m[','] = false;
      m[';'] = false;
      m[':'] = false;
      m['\\'] = false;
      m['"'] = false;
      m['/'] = false;
      m['['] = false;
      m[']'] = false;
      m['?'] = false;
      m['='] = false;
      m['{'] = false;
      m['}'] = false;
      m[' '] = false;
      m['\t'] = false;
      tokenChars = m;
    }


  }

}