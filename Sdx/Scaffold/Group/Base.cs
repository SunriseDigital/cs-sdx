using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public abstract class Base
  {
    public Base(string columnName)
    {
      this.TargetColumnName = columnName;
    }

    public string TargetColumnName { get; private set; }

    protected abstract string FetchName();

    public string Name
    {
      get
      {
        return FetchName();
      }
    }

    public string TargetValue { get; set; }

    internal Manager Manager { get; set; }
  }
}
