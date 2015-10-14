using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Util
{
  public static class Datetime
  {
    public static System.DateTime RoundTicks(System.DateTime dateTime)
    {
      return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
    }
  }
}
