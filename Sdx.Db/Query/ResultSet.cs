using System;
using System.Collections.Generic;

namespace Sdx.Db.Query
{
  public class ResultSet
  {
    private List<Result> results = new List<Result>();
    private Dictionary<string, Result> resultDic = new Dictionary<string, Result>();

    internal void Build(List<Dictionary<string, object>> list, Select select, string contextName)
    {
      Table table = select.Context(contextName).Table;

      list.ForEach(row => {
        var pkeys = table.Meta.Pkeys;
        if (pkeys == null)
        {
          throw new Exception("Missing Pkeys setting in " + table.ToString() + ".Meta");
        }

        var key = this.buildUniqueKey(row, pkeys, contextName);
        Result result;
        if (!this.resultDic.ContainsKey(key))
        {
          result = new Result();
          result.ContextName = contextName;
          result.Select = select;
          this.results.Add(result);
          this.resultDic[key] = result;
        }
        else
        {
          result = this.resultDic[key];
        }

        result.AddRow(row);

      });
    }

    private string buildUniqueKey(Dictionary<string, object> row, List<string> pkeys, string contextName)
    {
      var key = "";

      pkeys.ForEach(column => {
        if (key != "")
        {
          key += "%%SDX%%";
        }

        key += row[column + "@" + contextName];
      });

      return key;
    }

    public int Count
    {
      get
      {
        return this.results.Count;
      }
    }

    public Result First
    {
      get
      {
        return this.results[0];
      }
    }

    public Result this[int key]
    {
      get
      {
        return this.results[key];
      }
    }


    public void ForEach(Action<Result> action)
    {
      this.results.ForEach(action);
    }
  }
}