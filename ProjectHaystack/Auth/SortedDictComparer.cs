//
// Copyright (c) 2017, SkyFoundry LLC
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017 Hank Weber Creation
//

using System.Collections;
using System.Collections.Generic;

namespace ProjectHaystack.Auth
{
  class SortedDictComparer : IComparer<IDictionary>
  {
    public int Compare(IDictionary a, IDictionary b)
    {
      if (a.Count != b.Count)
      {
        return 0;
      }
      else
      {
        var aSorted = new SortedDictionary<string, string>();
        foreach(DictionaryEntry x in a)
        {
          aSorted[x.Key.ToString()] = x.Value.ToString();
        }
        var bSorted = new SortedDictionary<string, string>();
        foreach (DictionaryEntry x in b)
        {
          bSorted[x.Key.ToString()] = x.Value.ToString();
        }
        foreach (KeyValuePair<string, string> x in aSorted)
        {
          if (!x.Value.Equals(b[x.Key]))
          {
            return 0;
          }
        }

      }
      return 1;
    }
  }
}
