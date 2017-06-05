using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Sdx.Util
{
  public static class String
  {
    private const string RandomSeeds = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static readonly Dictionary<char, char> fullWidthNumberlist = new Dictionary<char, char>{
     {'１','1'},{'２','2'},{'３','3'},{'４','4'},{'５','5'},{'６','6'},{'７','7'},{'８','8'},{'９','9'},{'０','0'},
    };

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
    /// <param name="value">対象の文字列</param>
    /// <param name="max">文字数</param>
    /// <param name="substitute">最後に付ける文字列(デフォルトは「…」)</param>
    /// <returns>string カットされた文字列</returns>
    public static string Truncate(string value, int max, string substitute = "…")
    {
      return value.Length <= max ? value : value.Substring(0, max) + substitute;
    }

    public static string ToCamelCase(string value)
    {
      if (string.IsNullOrEmpty(value))
      {
        return value;
      }

      return value
          .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
          .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
          .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }

    public static string ToSnakeCase(string value)
    {
      return string.Concat(value.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
    }

    public static string RemoveFirstLines(string value, int linesCount)
    {
      var lines = Regex.Split(value, "\r\n|\r|\n").Skip(linesCount);
      return string.Join(Environment.NewLine, lines.ToArray());
    }

    public static bool IsEmpty(string value)
    {
      return value == null || value == "";
    }

    public static string StandardizeLineBreak(string value)
    {
      return value.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
    }

    public static string ReplaceLineBreak(string value, string replaceTo)
    {
      return value.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", replaceTo);
    }

    /// <summary>
    /// 文字列に含まれている全角数字を半角数字へ変換する。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ConvertFullWidthNumbersToHalfWidthNumbers(string value)
    {
      return new string(value.Select( chr => (fullWidthNumberlist.ContainsKey(chr) ? fullWidthNumberlist[chr] : chr )).ToArray());
    }

    /// <summary>
    /// QRコードを読むアプリケーションで正規表現が正しく機能せずリンクが生成されないケースがある為
    /// 本来はURLエンコードの必要のない記号（予約文字）も含めてエンコードします
    /// </summary>
    /// <param name="str"></param>
    /// <param name="enableReservedChar"></param>
    /// <returns></returns>
    public static string UrlEncode(string str, bool isEnableReservedChar = false)
    {
      // !()_-*.以外の文字列がエンコードされます
      str = HttpUtility.UrlEncode(str);

      if (isEnableReservedChar)
      {
        string reservedChars = "!*'();:@&=+$,/?#[]";

        var sb = new StringBuilder();

        foreach (char @char in str)
        {
          if (reservedChars.IndexOf(@char) == -1)
          {
            sb.Append(@char);
            continue;
          }

          sb.AppendFormat("%{0:X2}", (int)@char);
        }

        str = sb.ToString();
      }

      return str;
    }
  }
}
