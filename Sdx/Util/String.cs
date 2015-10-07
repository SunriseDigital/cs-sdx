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
  }
}
