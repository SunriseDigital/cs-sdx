using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Sdx.Db.Sql;

namespace Sdx.Db
{
  public class RecordSet : IEnumerable<Record>
  {
    //主キーでユニークなのでOrderedDictionaryを使っています。
    private Collection.OrderedDictionary<string, Record> resultDic = new Collection.OrderedDictionary<string, Record>();

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

      if (!pkeys.Any())
      {
        throw new NotImplementedException("Missing Pkeys setting in " + table.ToString() + ".Meta");
      }

      list.ForEach(row => {
        this.BuildResults(select, row, pkeys, contextName);
      });
    }

    private void BuildResults(Select select, Dictionary<string, object> row, IEnumerable<Table.Column> pkeys, string contextName)
    {
      //対象テーブルの主キーがNULLの場合（LEFTJOINの時）、関係ない行なのでスルーする
      var exists = true;
      foreach(var column in pkeys)
      {
        if (row[Record.BuildColumnAliasWithContextName(column.Name, contextName)] is DBNull)
        {
          exists = false;
          break;
        }
      }

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
        this.resultDic.Add(key, result);
      }
      else
      {
        result = this.resultDic[key];
      }

      result.AddRow(row);
    }


    private string BuildUniqueKey(Dictionary<string, object> row, IEnumerable<Table.Column> pkeys, string contextName)
    {
      var key = "";

      foreach(var column in pkeys)
      {
        if (key != "")
        {
          key += "%%SDX%%";
        }

        key += row[Record.BuildColumnAliasWithContextName(column.Name, contextName)];
      }

      return key;
    }

    public int Count
    {
      get
      {
        return this.resultDic.Count;
      }
    }

    public Record First
    {
      get
      {
        return this.resultDic.ItemAt(0);
      }
    }

    public Record this[int key]
    {
      get
      {
        return this.resultDic.ItemAt(key);
      }
    }

    public T Pop<T>(Predicate<T> match) where T : Sdx.Db.Record
    {
      var find = resultDic.FindIndex((kv) => match((T)kv.Value));
      if (find != -1)
      {
        var tmp = (T)resultDic.ItemAt(find);
        resultDic.RemoveAt(find);
        return tmp;
      }
      return null;
    }

    public Record Pop(Predicate<Record> match)
    {
      return Pop<Record>(match);
    }

    public void ForEach<T>(Action<T> action) where T : Sdx.Db.Record
    {
      this.resultDic.ForEach((key, rec) => action((T) rec));
    }

    public void ForEach(Action<Record> action)
    {
      this.resultDic.ForEach((key, rec) => action(rec));
    }

    public IEnumerator<Record> GetEnumerator()
    {
      foreach(var kv in resultDic)
      {
        yield return kv.Value;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public string[] ToStringArray<T>(Func<T, string> func) where T : Sdx.Db.Record
    {
      var result = new string[Count];
      int key = 0;
      ForEach(record =>
      {
        result.SetValue(func((T)record), key++);
      });

      return result;
    }

    public string[] ToStringArray(Func<Record, string> func)
    {
      return ToStringArray<Record>(func);
    }
  }
}