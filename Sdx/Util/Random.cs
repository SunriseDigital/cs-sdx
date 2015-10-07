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
  }
}
