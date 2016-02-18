using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sdx.Db.Sql;
using System.Collections.Specialized;

namespace Sdx.Db
{
  public abstract class Record
  {
    private Dictionary<string, object> recordCache = new Dictionary<string, object>();

    internal string ContextName { get; set; }
    internal Dictionary<string, object> UpdatedValues { get; private set; }
    internal List<Dictionary<string, object>> ValuesList { get; private set; }
    internal Select Select { get; set; }

    public Record()
    {
      this.UpdatedValues = new Dictionary<string, object>();
      this.ValuesList = new List<Dictionary<string, object>>();
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
      if (this.UpdatedValues.ContainsKey(key))
      {
        return this.UpdatedValues[key];
      }

      if (IsNew)
      {
        return null;
      }

      var keyWithContext = Record.BuildColumnAliasWithContextName(key, this.ContextName);
      if(!this.ValuesList[0].ContainsKey(keyWithContext))
      {
        return null;
      }
      var value = this.ValuesList[0][keyWithContext];

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

    public Record GetRecord(string contextName, Action<Select> selectHook = null)
    {
      return this.GetRecord(contextName, null, selectHook);
    }

    public Record GetRecord(string contextName, Connection connection, Action<Select> selectHook = null)
    {
      var records = this.GetRecordSet(contextName, connection, selectHook);
      if (records.Count == 0)
      {
        return null;
      }

      return records[0];
    }

    public RecordSet GetRecordSet(string contextName, Action<Select> selectHook = null)
    {
      return this.GetRecordSet(contextName, null, selectHook);
    }

    public RecordSet GetRecordSet(string contextName, Connection connection, Action<Select> selectHook = null)
    {
      if (this.recordCache.ContainsKey(contextName))
      {
        if (selectHook != null)
        {
          throw new ArgumentException("You must clear record cache, before use selectHook.");
        }

        return (RecordSet)this.recordCache[contextName];
      }

      if (this.Select.HasContext(contextName)) //already joined
      {
        if (selectHook != null)
        {
          throw new ArgumentException("You can't use selectHook, because already joined " + contextName + " context.");
        }

        var resultSet = new RecordSet();
        resultSet.Build(this.ValuesList, this.Select, contextName);
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

        var table = this.Select.Context(this.ContextName).Table;
        if (table.OwnMeta.Relations.ContainsKey(contextName))
        {
          var relations = table.OwnMeta.Relations[contextName];

          var sel = new Select(this.Select.Adapter);
          sel.SetComment(this.GetType().Name + "::GetRecordSet(" + contextName  + ")");
          sel.AddFrom(relations.TableMeta.CreateTable())
            .Where.Add(relations.ReferenceKey, this.GetString(relations.ForeignKey));

          if (selectHook != null)
          {
            selectHook.Invoke(sel);
          }

          RecordSet resultSet = connection.FetchRecordSet(sel);

          //キャッシュする
          this.recordCache[contextName] = resultSet;
          return resultSet;
        }

        throw new NotImplementedException("Missing relation setting for " + contextName);
      }
    }

    internal void AddRow(Dictionary<string, object> row)
    {
      this.ValuesList.Add(row);
    }

    internal static string BuildColumnAliasWithContextName(string columnName, string contextName)
    {
      return columnName + "@" + contextName;
    }

    public bool IsNew
    {
      get
      {
        return this.ValuesList.Count == 0;
      }
    }

    public bool IsUpdated
    {
      get
      {
        return this.UpdatedValues.Count != 0;
      }
    }

    public bool IsDeleted { get; internal set; }

    public void SetValue(string columnName, object value)
    {
      this.OwnMeta.CheckColumn(columnName);
      if(!value.Equals(this.GetValue(columnName)))
      {
        this.UpdatedValues[columnName] = value;
      }
    }

    internal void AppendPkeyWhere(Condition where)
    {
      if (this.OwnMeta.Pkeys.Count == 0)
      {
        throw new InvalidOperationException("Missing Pkey data in " + this.OwnMeta.Name + " table");
      }

      this.OwnMeta.Pkeys.ForEach(column =>
      {
        var value = this.GetValue(column);
        if (value == null)
        {
          throw new InvalidOperationException("Primary key " + column + " is null.");
        }
        where.Add(column, value);
      });
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

    public void Bind(NameValueCollection collection)
    {
      this.OwnMeta.Columns.ForEach((column) => {
        if(column.IsAutoIncrement)
        {
          return;
        }
        var value = collection[column.Name];
        if (value != null)
        {
          this.SetValue(column.Name, value);
        }
      });
    }

    public NameValueCollection ToNameValueCollection()
    {
      var col = new NameValueCollection();
      OwnMeta.Columns.ForEach((column) => {
        col.Add(column.Name, this.GetString(column.Name));
      });

      return col;
    }

    public T Get<T>(string path, Connection conn = null)
    {
      return (T)Get(path, conn);
    }


    /// <summary>
    /// `.`で区切って深い階層のデータを取得可能。`@`はGetRecord、`#`はメソッドを検索、それ以外はカラムの取得を実行します。
    /// メソッドに引数は渡せません。
    /// e.g.
    /// record.GetDynamic("@some_record.#GetSomeMethod.name"); = record.GetRecord("some_record").GetSomeMethod().GetValue("name");
    /// </summary>
    /// <param name="path"></param>
    /// <param name="conn"></param>
    /// <returns></returns>
    public dynamic Get(string path, Connection conn = null)
    {
      dynamic result = this;
      var chunk = path.Split('.');
      foreach(var key in chunk)
      {
        if (result == null)
        {
          throw new InvalidOperationException("Before " + key + " owner " + " is NULL in " + path);
        }

        //record
        if (key.StartsWith("@"))
        {
          result = result.GetRecord(key.Substring(1), conn);
        }
        else if(key.StartsWith("#"))
        {
          var method = key.Substring(1);
          System.Type type = result.GetType();
          result = type.GetMethods().First(m => m.Name == method && !m.IsStatic && m.GetParameters().Count() == 0).Invoke(result, null);
        }
        else
        {
          result = result.GetValue(key);
        }
      }
      return result;
    }
  }
}
