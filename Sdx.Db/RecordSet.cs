using System;
using System.Collections.Generic;
using System.Data.Common;

using Sdx.Db.Query;

namespace Sdx.Db
{
  public class RecordSet<T> where T : Record, new()
  {
    private List<T> results = new List<T>();
    private Dictionary<string, T> resultDic = new Dictionary<string, T>();

    internal void Build(DbDataReader reader, Select select, string contextName)
    {
      Table table = select.Context(contextName).Table;
      var pkeys = table.OwnMeta.Pkeys;
      if (pkeys == null)
      {
        throw new Exception("Missing Pkeys setting in " + table.ToString() + ".Meta");
      }

      while (reader.Read())
      {
        var row = new Dictionary<string, object>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
          row[reader.GetName(i)] = reader.GetValue(i);
        }

        this.BuildResults(select, row, pkeys, contextName);
      }
    }

    internal void Build(List<Dictionary<string, object>> list, Select select, string contextName)
    {
      Table table = select.Context(contextName).Table;
      var pkeys = table.OwnMeta.Pkeys;
      if (pkeys == null)
      {
        throw new Exception("Missing Pkeys setting in " + table.ToString() + ".Meta");
      }

      if (pkeys.Count == 0)
      {
        throw new Exception("Missing Pkeys setting in " + table.ToString() + ".Meta");
      }

      list.ForEach(row => {
        this.BuildResults(select, row, pkeys, contextName);
      });
    }

    private void BuildResults(Select select, Dictionary<string, object>  row, List<string> pkeys, string contextName)
    {
      //対象テーブルの主キーがNULLの場合（LEFTJOINの時）、関係ない行なのでスルーする
      var exists = true;
      pkeys.ForEach(column =>
      {
        if(row[column + "@" + contextName] is DBNull)
        {
          exists = false;
          return;
        }
      });

      if (!exists)
      {
        return;
      }

      var key = this.BuildUniqueKey(row, pkeys, contextName);
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
    }


    private string BuildUniqueKey(Dictionary<string, object> row, List<string> pkeys, string contextName)
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