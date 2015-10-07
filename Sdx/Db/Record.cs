using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sdx.Db.Sql;

namespace Sdx.Db
{
  public class Record
  {
    private Select select;

    private List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

    private Dictionary<string, object> recordCache = new Dictionary<string, object>();

    private Dictionary<string, object> updatedValues = new Dictionary<string, object>();

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
      if (this.updatedValues.ContainsKey(key))
      {
        return this.updatedValues[key];
      }

      if (IsNew)
      {
        return null;
      }

      var keyWithContext = Record.BuildColumnAliasWithContextName(key, this.ContextName);
      if(!this.list[0].ContainsKey(keyWithContext))
      {
        return null;
      }
      var value = this.list[0][keyWithContext];

      if(value is DBNull)
      {
        return null;
      }

      return value;
    }

    public string GetString(string key)
    {
      return Convert.ToString(this.GetValue(key));
    }

    public bool HasValue(string key)
    {
      return this.GetValue(key) != null;
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

    public T GetRecord<T>(string contextName, Action<Select> selectHook = null) where T : Record, new()
    {
      return this.GetRecord<T>(contextName, null, selectHook);
    }

    public T GetRecord<T>(string contextName, Connection connection, Action<Select> selectHook = null) where T : Record, new()
    {
      var records = this.GetRecordSet<T>(contextName, connection, selectHook);
      if (records.Count == 0)
      {
        return null;
      }

      return records[0];
    }

    public RecordSet<T> GetRecordSet<T>(string contextName, Action<Select> selectHook = null) where T : Record, new()
    {
      return this.GetRecordSet<T>(contextName, null, selectHook);
    }

    public RecordSet<T> GetRecordSet<T>(string contextName, Connection connection, Action<Select> selectHook = null) where T : Record, new()
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
        if (connection == null)
        {
          throw new ArgumentNullException("connection");
        }

        var table = this.select.Context(this.ContextName).Table;
        if (table.OwnMeta.Relations.ContainsKey(contextName))
        {
          var relations = table.OwnMeta.Relations[contextName];

          var sel = new Select(this.select.Adapter);
          sel.SetComment(this.GetType().Name + "::GetRecordSet(" + contextName  + ")");
          sel.AddFrom((Table)Activator.CreateInstance(relations.TableType))
            .Where.Add(relations.ReferenceKey, this.GetString(relations.ForeignKey));

          if (selectHook != null)
          {
            selectHook.Invoke(sel);
          }

          RecordSet<T> resultSet = connection.FetchRecordSet<T>(sel);

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

    public bool IsNew
    {
      get
      {
        return this.list.Count == 0;
      }
    }

    public bool IsUpdated
    {
      get
      {
        return this.updatedValues.Count != 0;
      }
    }

    public void SetValue(string columnName, object value)
    {
      this.OwnMeta.CheckColumn(columnName);
      if(!value.Equals(this.GetValue(columnName)))
      {
        this.updatedValues[columnName] = value;
      }
    }

    public void Save(Connection conn)
    {
      if(updatedValues.Count == 0)
      {
        return;
      }

      if (this.IsNew)
      {
        var insert = conn.Adapter.CreateInsert();
        insert.SetInto(this.OwnMeta.Name);

        foreach (var columnValue in this.updatedValues)
        {
          insert.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        conn.Execute(insert);

        //値を保存後も取得できるようにする
        var newValues = new Dictionary<string, object>();
        foreach(var columnValue in this.updatedValues)
        {
          var key = Record.BuildColumnAliasWithContextName(columnValue.Key, this.ContextName);
          newValues[key] = columnValue.Value;
        }

        //保存に成功し、PkeyがNullだったらAutoincrementのはず。
        //Autoincrementは通常テーブルに１つしか作れないはず（MySQLとSQLServerはそうだった）
        var pkey = this.OwnMeta.Pkeys[0];
        var pkeyValue = this.GetValue(pkey);
        if (pkeyValue == null)
        {
          var key = Record.BuildColumnAliasWithContextName(pkey, this.ContextName);
          newValues[key] = conn.FetchLastInsertId();
        }

        this.list.Add(newValues);
      }
      else
      {
        var update = conn.Adapter.CreateUpdate();
        update.SetTable(this.OwnMeta.Name);
        foreach (var columnValue in this.updatedValues)
        {
          update.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        if(this.OwnMeta.Pkeys.Count == 0)
        {
          throw new InvalidOperationException("Missing Pkey data in " + this.OwnMeta.Name + " table");
        }

        this.OwnMeta.Pkeys.ForEach(column =>
        {
          var value = this.GetValue(column);
          if(value == null)
          {
            throw new InvalidOperationException("Primary key " + column + " is null.");
          }
          update.Where.Add(column, value);
        });

        conn.Execute(update);

        //値を保存後も取得できるようにする
        foreach(var row in this.list)
        {
          foreach(var columnValue in updatedValues)
          {
            var key = Record.BuildColumnAliasWithContextName(columnValue.Key, this.ContextName);
            row[key] = columnValue.Value;
          }
        }
      }

      this.updatedValues.Clear();
    }

    public override string ToString()
    {
      var builder = new StringBuilder();
      builder
        .Append(this.OwnMeta.Name)
        .Append(": {")
        ;
      this.OwnMeta.Columns.ForEach(column => 
      {
        builder
          .Append(column.Name)
          .Append(": ")
          .Append('"')
          .Append(this.GetString(column.Name))
          .Append("\", ")
          ;
      });

      builder
        .Remove(builder.Length - 2, 2)
        .Append("}");

      return builder.ToString();
    }
  }
}
