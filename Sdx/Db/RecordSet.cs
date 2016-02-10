using System;
using System.Collections.Generic;
using System.Data.Common;

using Sdx.Db.Sql;

namespace Sdx.Db
{
  public class RecordSet : IEnumerable<Record>
  {
    private List<Record> results = new List<Record>();
    private Dictionary<string, Record> resultDic = new Dictionary<string, Record>();

    internal void Build(DbDataReader reader, Select select, string contextName)
    {
      Table table = select.Context(contextName).Table;
      if(table == null)
      {
        throw new InvalidOperationException("Use Sdx.Db.Table, if you want to get Record.");
      }

      var pkeys = table.OwnMeta.Pkeys;
      if (pkeys == null)
      {
        throw new NotImplementedException("Missing Pkeys setting in " + table.ToString() + ".Meta");
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
        throw new NotImplementedException("Missing Pkeys setting in " + table.ToString() + ".Meta");
      }

      if (pkeys.Count == 0)
      {
        throw new NotImplementedException("Missing Pkeys setting in " + table.ToString() + ".Meta");
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
        if (row[Record.BuildColumnAliasWithContextName(column, contextName)] is DBNull)
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
      Record result;
      if (!this.resultDic.ContainsKey(key))
      {
        result = select.Context(contextName).Table.OwnMeta.CreateRecord();
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

        key += row[Record.BuildColumnAliasWithContextName(column, contextName)];
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

    public Record First
    {
      get
      {
        return this.results[0];
      }
    }

    public Record this[int key]
    {
      get
      {
        return this.results[key];
      }
    }


    public void ForEach(Action<Record> action)
    {
      this.results.ForEach(action);
    }

    public IEnumerator<Record> GetEnumerator()
    {
      return this.results.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.results.GetEnumerator();
    }
  }
}