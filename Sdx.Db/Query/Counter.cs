using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Query
{
  public class Counter
  {
    private int value = 0;
    public int Value { get { return this.value; } }
    public void Incr()
    {
      ++this.value;
    }
  }
}
