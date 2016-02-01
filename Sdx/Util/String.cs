using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Util
{
  public static class String
  {
    private const string RandomSeeds = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenRandom(int length)
    {
      var rnd = Number.CreateRandom();
      var builder = new StringBuilder();
      for(var i = 0; i < length; i++)
      {
        var index = rnd.Next(0, RandomSeeds.Length - 1);
        builder.Append(RandomSeeds[index]);
      }

      return builder.ToString();
    }

    /// <summary>
    /// 文字列を指定した数(インデックス)から最後まで削除し末尾に代替テキストを挿入します
    /// </summary>
    /// <param name="text">対象の文字列</param>
    /// <param name="max">文字数</param>
    /// <param name="substitute">最後に付ける文字列(デフォルトは「…」)</param>
    /// <returns>string カットされた文字列</returns>
    public static string Truncate(string text, int max, string substitute = "…")
    {
      return text.Length <= max ? text : text.Substring(0, max) + substitute;
    }

    public static string ToCamelCase(string str)
    {
      if (string.IsNullOrEmpty(str))
      {
        return str;
      }

      return str
          .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
          .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
          .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }
  }
}
