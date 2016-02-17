using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

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
      var pairList = GetPairListForSelector();
      if(pairList == null)
      {
        return null;
      }

      var select = new Html.Select();
      select.Name = TargetColumnName;

      if(!Strict)
      {
        //TODO I18n
        select.AddOption(Html.Option.Create("", "全て"));
      }

      pairList.ForEach((pair) =>
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

    private bool initialized = false;

    public void Init()
    {
      if (initialized)
      {
        return;
      }

      initialized = true;

      if (FixedValue != null)
      {
        if (HasSelector)
        {
          throw new InvalidOperationException("You can't use FixedValue and Selector at the same time.");
        }

        if (DefaultValue != null)
        {
          throw new InvalidOperationException("You can't use FixedValue and DefaultValue at the same time.");
        }

        TargetValue = FixedValue;
      }
      else
      {
        TargetValue = HttpContext.Current.Request.QueryString[TargetColumnName];
      }


      Html.Select selector = null;
      if (HasSelector)
      {
        selector = BuildSelector();
      }

      if (TargetValue != null)
      {
        Manager.ListPageUrl.AddParam(TargetColumnName, TargetValue);
        Manager.EditPageUrl.AddParam(TargetColumnName, TargetValue);
      }
      else if (Strict)
      {
        if (DefaultValue != null || selector != null)
        {
          string value = null;
          if (DefaultValue != null)
          {
            value = DefaultValue;
          }
          else if (selector != null)
          {
            value = selector.Options.First().Tag.Attr["value"];
          }

          Manager.ListPageUrl.AddParam(TargetColumnName, value);
          HttpContext.Current.Response.Redirect(Manager.ListPageUrl.Build(), true);
        }


        if (Sdx.Context.Current.HttpErrorHandler.HasHandler(404))
        {
          Sdx.Context.Current.HttpErrorHandler.Invoke(404);
        }
        else
        {
          throw new HttpException(404, "Missing " + TargetColumnName + " parameter");
        }
      }
    }
  }
}
