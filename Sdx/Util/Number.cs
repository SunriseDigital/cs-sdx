using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Sdx.Util
{
  public static class Number
  {
    private static ThreadLocal<Random> randomWrapper = new ThreadLocal<System.Random>(() =>
    {
      using (var rng = new RNGCryptoServiceProvider())
      {
        var buffer = new byte[sizeof(int)];
        rng.GetBytes(buffer);
        var seed = BitConverter.ToInt32(buffer, 0);
        return new Random(seed);
      }
    });

    /// <summary>
    /// スレッドセイフでEnviroment.TickCountに左右されないRandomを生成します。
    /// </summary>
    /// <returns></returns>
    public static Random CreateRandom()
    {
      return randomWrapper.Value;
    }

    /// <summary>
    /// 数字から1st 2nd 3rd...を返す。
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static string ToOrdinal(int num)
    {
      if (num <= 0) return num.ToString();

      switch (num % 100)
      {
        case 11:
        case 12:
        case 13:
          return num + "th";
      }

      switch (num % 10)
      {
        case 1:
          return num + "st";
        case 2:
          return num + "nd";
        case 3:
          return num + "rd";
        default:
          return num + "th";
      }
    }
  }
}
