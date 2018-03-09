using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx
{
  public class SplittableText
  {
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="targetText">対象文字列</param>
    /// <param name="boundary">区切り文字</param>
    public SplittableText(string targetText, string boundary = "@Sdx.SplittableText.Boundary@")
    {
      RawText = targetText;
      Boundary = boundary;
      parts = RawText
        .Split(new string[] { Boundary }, StringSplitOptions.None)
        .Select(x => x.Trim()).ToArray<string>()
      ;
    }

    /// <summary>
    /// 対象文字列を区切って配列にする際に使う区切り文字列
    /// </summary>
    public string Boundary
    {
      get;
      private set;
    }

    /// <summary>
    /// 元の文字列
    /// </summary>
    public string RawText
    {
      get;
      private set;
    }

    /// <summary>
    /// RawText を Boundary で区切って配列にしたもの
    /// </summary>
    private string[] parts;

    public int PartCount
    {
      get
      {
        return parts.Length;
      }
    }

    public bool HasMultipleParts
    {
      get
      {
        return PartCount > 1;
      }
    }

    public string First
    {
      get
      {
        return parts.FirstOrDefault();
      }
    }

    public string Last
    {
      get
      {
        return parts.LastOrDefault();
      }
    }

    /// <summary>
    /// Partsから引数で指定されたn番目の要素を返す(先頭なら 1 が引数になる)
    /// 存在しない要素の場合は null を返す
    /// </summary>
    /// <param name="nth"></param>
    /// <returns></returns>
    public string PartAt(int nth)
    {
      if (nth < 1 || nth > parts.Length)
      {
        return null;
      }

      return parts[nth - 1];
    }
  }
}
