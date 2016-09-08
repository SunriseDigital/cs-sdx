using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Sdx.Db.Sql;

namespace Sdx.Db
{
  public class RecordSet : IEnumerable<Record>
  {
    private static Random random = Util.Number.CreateRandom();

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
        var rowKey = Record.BuildColumnAliasWithContextName(column.Name, contextName);
        if (!row.ContainsKey(rowKey))
        {
          throw new InvalidOperationException("Missing " + rowKey + " in " + Sdx.Diagnostics.Debug.Dump(row));
        }

        if (row[rowKey] is DBNull)
        {
          exists = false;
          break;
        }
      }

      if (!exists)
      {
        return;
      }

      var key = BuildUniqueKey(pkeys, columnName => row[Record.BuildColumnAliasWithContextName(columnName, contextName)]);
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


    private string BuildUniqueKey(IEnumerable<Table.Column> pkeys, Func<string, object> func)
    {
      var key = "";

      foreach(var column in pkeys)
      {
        if (key != "")
        {
          key += "%%SDX%%";
        }
        var pkeyValue = func(column.Name);
        if(pkeyValue == null)
        {
          throw new InvalidOperationException(column.Name + " value is null. Is this new record ?");
        }
        key += pkeyValue;
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

    public Record Pop()
    {
      var tmp = resultDic.ItemAt(0);
      resultDic.RemoveAt(0);
      return tmp;
    }

    public RecordSet PopSet(int count)
    {
      var results = new RecordSet();
      for (int i = 0; i < count; i++)
      {
        results.AddRecord(Pop());
      }
      return results;
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

    public T Get<T>(int index) where T:Sdx.Db.Record
    {
      return (T)this[index];
    }

    public void AddRecord(Record record)
    {
      var key = BuildUniqueKey(record.OwnMeta.Pkeys, column => record.GetValue(column));
      this.resultDic.Add(key, record);
    }

    public RecordSet Sort(Comparison<Record> comp)
    {
      resultDic.Sort(comp);
      return this;
    }

    private Dictionary<string, Dictionary<int, RecordSet>> groupByColumnCacheForInt = new Dictionary<string, Dictionary<int, RecordSet>>();
    private Dictionary<string, Dictionary<string, RecordSet>> groupByColumnCacheForString = new Dictionary<string, Dictionary<string, RecordSet>>();

    public RecordSet GroupByColumn(string column, int value)
    {
      if(!groupByColumnCacheForInt.ContainsKey(column))
      {
        var cache = new Dictionary<int, RecordSet>();
        groupByColumnCacheForInt[column] = cache;
        ForEach(rec => 
        {
          var val = rec.GetInt32(column);
          if(!cache.ContainsKey(val))
          {
            cache[val] = new RecordSet();
          }

          cache[val].AddRecord(rec);
        });
      }

      if(groupByColumnCacheForInt[column].ContainsKey(value))
      {
        return groupByColumnCacheForInt[column][value];
      }
      else
      {
        return new RecordSet();
      }
    }

    public RecordSet GroupByColumn(string column, string value)
    {
      if (!groupByColumnCacheForString.ContainsKey(column))
      {
        var cache = new Dictionary<string, RecordSet>();
        groupByColumnCacheForString[column] = cache;
        ForEach(rec =>
        {
          var val = rec.GetString(column);
          if (!cache.ContainsKey(val))
          {
            cache[val] = new RecordSet();
          }

          cache[val].AddRecord(rec);
        });
      }

      if (groupByColumnCacheForString[column].ContainsKey(value))
      {
        return groupByColumnCacheForString[column][value];
      }
      else
      {
        return new RecordSet();
      }
    }

    public RecordSet ClearGroupByColumnCache(string column)
    {
      groupByColumnCacheForString.Remove(column);
      groupByColumnCacheForInt.Remove(column);
      return this;
    }

    public RecordSet Shuffle()
    {
      Sort((rec1, rec2) => random.Next(-1, 1));
      return this;
    }
  }
}