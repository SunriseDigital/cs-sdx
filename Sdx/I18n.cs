using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx
{
  public class I18n
  {
    public static string GetString(params string[] key)
    {
      return key[0];
    }

    public static string GetPluralString(string key, params int[] counts)
    {
      return key;
    }

    public static string GetQuotedString(params string[] key)
    {
      return "\"" + key[0] + "\"";
    }
  }
}
