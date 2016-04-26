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

    private List<KeyValuePair<string, string>> pairListForSelector;

    internal protected abstract List<KeyValuePair<string, string>> BuildPairListForSelector(Sdx.Db.Connection conn);

    private List<KeyValuePair<string, string>> GetPairListForSelector(Sdx.Db.Connection conn)
    {
      if (pairListForSelector == null)
      {
        pairListForSelector = BuildPairListForSelector(conn);
        if(pairListForSelector == null)
        {
          pairListForSelector = new List<KeyValuePair<string, string>>();
        }
      }

      return pairListForSelector;
    }

    public Html.Select BuildSelector(Sdx.Db.Connection conn)
    {
      var pairList = GetPairListForSelector(conn);
      if(pairList.Count == 0)
      {
        return null;
      }

      var select = new Html.Select();
      select.Name = TargetColumnName;

      if(!Strict)
      {
        select.AddOption("", Sdx.I18n.GetString("絞り込む"));
      }

      pairList.ForEach((pair) =>
      {
        select.AddOption(pair);
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

      if (TargetValue != null)
      {
        Manager.ListPageUrl.AddParam(TargetColumnName, TargetValue);
        Manager.EditPageUrl.AddParam(TargetColumnName, TargetValue);
      }
      else if (Strict)
      {
        //リダイレクト
        if (DefaultValue != null || HasSelector)
        {
          string value = null;
          if (DefaultValue != null)
          {
            value = DefaultValue;
          }

          Manager.ListPageUrl.AddParam(TargetColumnName, value);
          HttpContext.Current.Response.Redirect(Manager.ListPageUrl.Build(), true);
        }

        //404
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
