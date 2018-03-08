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
    public SplittableText(string targetText, string boundary = "@Sdx.SplittableString.Boundary@")
    {
      RawText = targetText;
      Boundary = boundary;
      Parts = RawText
        .Split(new string[] { Boundary }, StringSplitOptions.None)
        .Select(x => x.Trim()).ToArray<string>()
      ;
      HasBoundaryString = (Parts.Any() && Parts.Skip(1).Any());
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
    public string[] Parts
    {
      get;
      private set;
    }

    /// <summary>
    /// 元の文字列に Boundary と同じ文字列があるかのフラグ
    /// </summary>
    public bool HasBoundaryString
    {
      get;
      private set;
    }

    public string First
    {
      get
      {
        return Parts.FirstOrDefault();
      }
    }

    public string Last
    {
      get
      {
        return Parts.LastOrDefault();
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
      if (nth < 1 || nth > Parts.Length)
      {
        return null;
      }

      return Parts[nth - 1];
    }
  }
}
