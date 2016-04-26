using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sdx.Diagnostics
{
  public class Bench
  {
    private Stopwatch timer;

    public Bench()
    {
      timer = new Stopwatch();
      timer.Start();
    }

    public string ElapseSec
    {
      get
      {
        return Debug.FormatStopwatchTicks(timer.ElapsedTicks);
      }
    }
  }
}
