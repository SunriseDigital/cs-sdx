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

    /// <summary>
    /// 時間を変更した、あたらしいDateTimeを返します。
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="hours"></param>
    /// <param name="minutes"></param>
    /// <param name="seconds"></param>
    /// <param name="milliseconds"></param>
    /// <returns></returns>
    public static System.DateTime ChangeTime(System.DateTime dateTime, int hours, int minutes, int seconds, int milliseconds = 0)
    {
      return new DateTime(
        dateTime.Year,
        dateTime.Month,
        dateTime.Day,
        hours,
        minutes,
        seconds,
        milliseconds,
        dateTime.Kind);
    }
  }
}
