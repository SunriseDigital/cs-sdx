using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Collection
{
  public class NameValueCollection : System.Collections.Specialized.NameValueCollection
  {
    public NameValueCollection()
    {

    }

    public NameValueCollection(System.Collections.Specialized.NameValueCollection nameValueCollection)
      : base(nameValueCollection)
    {

    }

    public IEnumerable<Dictionary<string, string>> GetDictionaryValues(string groupKey, string delim = "@")
    {
      var result  = new List<Dictionary<string, string>>();
      var prefix = groupKey + delim;
      var keys = AllKeys.Where(k => k.StartsWith(prefix));

      foreach (var key in keys)
      {
        var index = 0;
        foreach(var value in GetValues(key))
        {
          var dic = result.ElementAtOrDefault(index);
          if(dic == null)
          {
            dic = new Dictionary<string, string>();
            result.Add(dic);
          }

          dic[key.Substring(prefix.Length)] = value;

          ++index;
        }
      }

      return result;
    }
  }
}
