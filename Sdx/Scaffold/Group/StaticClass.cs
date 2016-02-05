using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public class StaticClass : Base
  {
    public StaticClass(string columnName, string className, string methodFordisplay, string methodForList)
      : base(columnName)
    {

    }

    protected override string FetchName()
    {
      return "StaticClass";
    }
  }
}
