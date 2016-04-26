using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Data
{
  public static class LargeArea
  {
    private static List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>() { 
      new KeyValuePair<string, string>("1", "東京"),
      new KeyValuePair<string, string>("2", "愛知"),
    };

    public static string GetName(string key)
    {
      var keyValue = values.First(kv => kv.Key == key);
      return keyValue.Value;
    }

    public static List<KeyValuePair<string, string>> GetList()
    {
      return values;
    }
  }
}
