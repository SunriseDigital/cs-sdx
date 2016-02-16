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
      this.Strict = false;
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

    internal protected abstract List<KeyValuePair<string, string>> GetPairListForSelector();

    public Html.Select BuildSelector()
    {
      var select = new Html.Select();
      select.Name = TargetColumnName;

      if(!Strict)
      {
        //TODO I18n
        select.AddOption(Html.Option.Create("", "全て"));
      }
      
      GetPairListForSelector().ForEach((pair) =>
      {
        select.AddOption(Html.Option.Create(pair));
      });

      if(TargetValue != null)
      {
        select.Bind(TargetValue);
      }

      return select;
    }

    public bool Strict { get; set; }

    public string DefaultValue { get; set; }

    public string FixedValue { get; set; }
  }
}
