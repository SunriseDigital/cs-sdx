using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public abstract class Base
  {
    public Base(string columnName, bool hasSelector)
    {
      this.TargetColumnName = columnName;
      this.HasSelector = hasSelector;
    }

    public string TargetColumnName { get; private set; }

    public bool HasSelector { get; private set; }

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

    internal protected abstract List<KeyValuePair<string, string>> GetKeyValuePairList();

    public Html.Select BuildSelector()
    {
      var select = new Html.Select();
      //TODO I18n
      select.AddOption(Html.Option.Create("", "全て"));
      select.Name = TargetColumnName;
      GetKeyValuePairList().ForEach((pair) =>
      {
        select.AddOption(Html.Option.Create(pair));
      });

      return select;
    }
  }
}
