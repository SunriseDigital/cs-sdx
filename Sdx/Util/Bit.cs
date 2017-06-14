using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Util
{
  public static class Bit
  {
    /// <summary>
    /// flagsにフラグ立っているか否かを調べます。
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static bool HasBit(int flags, int flag)
    {
      return (flags & flag ) == flag;;
    }
  }
}