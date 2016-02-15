using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public class StaticClass : Base
  {
    public StaticClass(string columnName, string className, string methodFordisplay, string methodForList = null)
      : base(columnName, methodForList != null)
    {

    }

    protected override string FetchName()
    {
      return "StaticClass";
    }

    protected internal override List<KeyValuePair<string, string>> GetPairListForSelector()
    {
      return null;
    }
  }
}
