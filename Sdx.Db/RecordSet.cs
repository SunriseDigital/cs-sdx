using System;
using System.Collections.Generic;

using Sdx.Db.Query;

namespace Sdx.Db
{
  public class RecordSet<T> where T : Record, new()
  {
    private List<T> results = new List<T>();
    private Dictionary<string, T> resultDic = new Dictionary<string, T>();

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
        T result;
        if (!this.resultDic.ContainsKey(key))
        {
          result = new T();
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

    public T First
    {
      get
      {
        return this.results[0];
      }
    }

    public T this[int key]
    {
      get
      {
        return this.results[key];
      }
    }


    public void ForEach(Action<T> action)
    {
      this.results.ForEach(action);
    }
  }
}