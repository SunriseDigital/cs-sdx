﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sdx.Db.Query;

namespace Sdx.Db
{
  public class Record
  {
    private Select select;

    private List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

    private Dictionary<string, object> recordCache = new Dictionary<string, object>();

    internal string ContextName { get; set; }

    internal Select Select
    {
      get
      {
        return this.select;
      }
      set
      {
        this.select = value;
      }
    }

    public TableMeta OwnMeta
    {
      get
      {
        var prop = this.GetType().GetProperty("Meta");
        if (prop == null)
        {
          throw new NotImplementedException("Missing Meta property in " + this.GetType());
        }

        var meta = prop.GetValue(null, null) as TableMeta;
        if (meta == null)
        {
          throw new NotImplementedException("Initialize TableMeta for " + this.GetType());
        }

        return meta;
      }
    }

    public DateTime GetDateTime(string key)
    {
      return Convert.ToDateTime(this.GetValue(key));
    }

    public decimal GetDecimal(string key)
    {
      return Convert.ToDecimal(this.GetValue(key));
    }

    public double GetDouble(string key)
    {
      return Convert.ToDouble(this.GetValue(key));
    }

    public short GetInt16(string key)
    {
      return Convert.ToInt16(this.GetValue(key));
    }

    public int GetInt32(string key)
    {
      return Convert.ToInt32(this.GetValue(key));
    }

    public long GetInt64(string key)
    {
      return Convert.ToInt64(this.GetValue(key));
    }

    public object GetValue(string key)
    {
      var keyWithContext = Record.BuildColumnAliasWithContextName(key, this.ContextName);
      if(!this.list[0].ContainsKey(keyWithContext))
      {
        throw new KeyNotFoundException("Missing " + keyWithContext + " key.");
      }
      return this.list[0][keyWithContext];
    }

    public string GetString(string key)
    {
      return Convert.ToString(this.GetValue(key));
    }

    public T GetRecord<T>(string contextName) where T : Record, new()
    {
      var records = this.GetRecordSet<T>(contextName);
      if (records.Count == 0)
      {
        return null;
      }

      return records[0];
    }

    public Record ClearRecordCache(string contextName = null)
    {
      if (contextName != null)
      {
        if (this.recordCache.ContainsKey(contextName))
        {
          this.recordCache.Remove(contextName);
        }
      }
      else
      {
        this.recordCache.Clear();
      }

      return this;
    }

    public RecordSet<T> GetRecordSet<T>(string contextName, Action<Select> selectHook = null) where T : Record, new()
    {
      if (this.recordCache.ContainsKey(contextName))
      {
        if (selectHook != null)
        {
          throw new ArgumentException("You must clear record cache, before use selectHook.");
        }

        return (RecordSet<T>)this.recordCache[contextName];
      }

      if (this.select.HasContext(contextName)) //already joined
      {
        if (selectHook != null)
        {
          throw new ArgumentException("You can't use selectHook, because already joined " + contextName + " context.");
        }

        var resultSet = new RecordSet<T>();
        resultSet.Build(this.list, this.select, contextName);
        //キャッシュする
        this.recordCache[contextName] = resultSet;
        return resultSet;
      }
      else //no join
      {
        var table = this.select.Context(this.ContextName).Table;
        if (table.OwnMeta.Relations.ContainsKey(contextName))
        {
          var relations = table.OwnMeta.Relations[contextName];

          var sel = new Select();
          sel.SetComment(this.GetType().Name + "::GetRecordSet(" + contextName  + ")");
          sel.Adapter = this.select.Adapter;
          sel.AddFrom((Table)Activator.CreateInstance(relations.TableType))
            .Where.Add(relations.ReferenceKey, this.GetString(relations.ForeignKey));

          if (selectHook != null)
          {
            selectHook.Invoke(sel);
          }

          var resultSet = this.select.Adapter.FetchRecordSet<T>(sel);

          //キャッシュする
          this.recordCache[contextName] = resultSet;
          return resultSet;
        }

        throw new NotImplementedException("Missing relation setting for " + contextName);
      }
    }

    internal void AddRow(Dictionary<string, object> row)
    {
      this.list.Add(row);
    }

    internal static string BuildColumnAliasWithContextName(string columnName, string contextName)
    {
      return columnName + "@" + contextName;
    }
  }
}