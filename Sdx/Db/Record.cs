using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sdx.Db.Sql;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace Sdx.Db
{
  public abstract class Record
  {
    public static string AutoCreateDateColumn { get; set; }
    public static string AutoUpdateDateColumn { get; set; }

    static Record()
    {
      AutoCreateDateColumn = "created_at";
      AutoUpdateDateColumn = "updated_at";
    }

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
        return DBNull.Value;
      }

      var keyWithContext = Record.BuildColumnAliasWithContextName(key, this.ContextName);
      if(!this.ValuesList[0].ContainsKey(keyWithContext))
      {
        return DBNull.Value;
      }
      var value = this.ValuesList[0][keyWithContext];

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

    public T GetRecord<T>(string contextName, Action<Select> selectHook = null) where T : Sdx.Db.Record
    {
      return this.GetRecord<T>(contextName, null, selectHook);
    }

    public T GetRecord<T>(string contextName, Connection connection, Action<Select> selectHook = null) where T : Sdx.Db.Record
    {
      var records = this.GetRecordSet(contextName, connection, selectHook);
      if (records.Count == 0)
      {
        return null;
      }

      return (T)records[0];
    }

    public Record GetRecord(string contextName, Action<Select> selectHook = null)
    {
      return this.GetRecord<Record>(contextName, null);
    }

    public Record GetRecord(string contextName, Connection connection, Action<Select> selectHook = null)
    {
      return this.GetRecord<Record>(contextName, connection, selectHook);
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

      if (this.Select != null && this.Select.HasContext(contextName)) //already joined
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

        if (OwnMeta.Relations.ContainsKey(contextName))
        {
          var relations = OwnMeta.Relations[contextName];

          var sel = connection.Adapter.CreateSelect();
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

    private bool EqualsToCurrent(string columnName, object value)
    {
      var current = GetValue(columnName);
      if(value == null)
      {
        return current == null;
      }

      if(current == null)
      {
        return value == null;
      }

      //DateTimeはミリ秒まで持っていてEquals更新していなくてもFalseを返し必ず更新されてしまうので秒までで比較します。
      if(current is DateTime)
      {
        return current.ToString().Equals(value.ToString());
      }

      if(current is DBNull)
      {
        return current == value;
      }
      
      value = Convert.ChangeType(value, current.GetType());
      return current.Equals(value);
    }

    public void SetValue(string columnName, object value)
    {
      this.OwnMeta.CheckColumn(columnName);
      if (!EqualsToCurrent(columnName, value))
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
        var value = this.GetValue(column.Name);
        if(value != null)
        {
          col.Add(column.Name, this.GetString(column.Name));
        }
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
        //record
        if (key.StartsWith("@"))
        {
          result = result.GetRecord(key.Substring(1), conn);
        }
        else if(key.StartsWith("#"))
        {
          var method = key.Substring(1);
          System.Type type = result.GetType();
          var methodInfo = type.GetMethods().FirstOrDefault(m => m.Name == method && !m.IsStatic);
          if (methodInfo == null)
          {
            throw new NotImplementedException("Missing " + method + " method in " + GetType());
          }

          var paramsCount = methodInfo.GetParameters().Count();
          if (paramsCount == 0)
          {
            result = methodInfo.Invoke(result, null);
          }
          else if (paramsCount == 1)
          {
            result = methodInfo.Invoke(result, new object[] { conn });
          }
          else
          {
            throw new NotSupportedException(method + "'s parameter must be nothing or Sdx.Db.Connection.");
          }
        }
        else
        {
          result = result.GetValue(key);
        }

        if (result == null)
        {
          break;
        }
        
      }
      return result;
    }

    public Dictionary<string, object> GetPkeyValues()
    {
      var dic = new Dictionary<string, object>();
      OwnMeta.Pkeys.ForEach((col) => dic[col] = GetValue(col));

      return dic;
    }

    public bool IsNull(string columnName)
    {
      return GetValue(columnName) == DBNull.Value;
    }

    private bool NeedsAutoUpdate(string columnName)
    {
      if (columnName == null)
      {
        return false;
      }

      if (!OwnMeta.HasColumn(columnName))
      {
        return false;
      }

      var first = UpdatedValues.FirstOrDefault(kv => kv.Key == columnName);
      if (first.Value == null)
      {
        return true;
      }

      if (first.Value.ToString() == "")
      {
        return true;
      }

      return false;
    }

    public void Save(Db.Connection conn)
    {
      if (IsDeleted)
      {
        throw new InvalidOperationException("This record is already deleted.");
      }

      if (IsNew)
      {
        var insert = conn.Adapter.CreateInsert();
        insert.SetInto(OwnMeta.Name);

        //自動登録日時更新
        if (NeedsAutoUpdate(AutoCreateDateColumn))
        {
          SetValue(AutoCreateDateColumn, DateTime.Now);
        }

        //自動更新日時更新
        if (NeedsAutoUpdate(AutoUpdateDateColumn))
        {
          SetValue(AutoUpdateDateColumn, DateTime.Now);
        }

        foreach (var columnValue in UpdatedValues)
        {
          insert.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        conn.Execute(insert);

        //値を保存後も取得できるようにする
        var newValues = new Dictionary<string, object>();
        foreach (var columnValue in UpdatedValues)
        {
          var key = Record.BuildColumnAliasWithContextName(columnValue.Key, ContextName);
          newValues[key] = columnValue.Value;
        }

        //保存に成功し、PkeyがNullだったらAutoincrementのはず。
        //Autoincrementは通常テーブルに１つしか作れないはず（MySQLとSQLServerはそうだった）
        var pkey = OwnMeta.Pkeys[0];
        var pkeyValue = GetValue(pkey);
        if (pkeyValue == DBNull.Value)
        {
          var key = Record.BuildColumnAliasWithContextName(pkey, ContextName);
          newValues[key] = conn.FetchLastInsertId();
        }

        ValuesList.Add(newValues);
      }
      else
      {
        if (UpdatedValues.Count == 0)
        {
          return;
        }

        var update = conn.Adapter.CreateUpdate();
        update.SetTable(OwnMeta.Name);
        foreach (var columnValue in UpdatedValues)
        {
          update.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        AppendPkeyWhere(update.Where);

        //自動更新日時更新
        if (
          UpdatedValues.Count > 0
          &&
          NeedsAutoUpdate(AutoUpdateDateColumn)
        )
        {
          SetValue(AutoUpdateDateColumn, DateTime.Now);
        }

        conn.Execute(update);

        //値を保存後も取得できるようにする
        foreach (var row in ValuesList)
        {
          foreach (var columnValue in UpdatedValues)
          {
            var key = Record.BuildColumnAliasWithContextName(columnValue.Key, ContextName);
            row[key] = columnValue.Value;
          }
        }
      }

      UpdatedValues.Clear();
    }

    public void Delete(Db.Connection conn)
    {
      if (IsNew)
      {
        return;
      }

      var delete = conn.Adapter.CreateDelete();
      delete.From = OwnMeta.Name;
      AppendPkeyWhere(delete.Where);

      conn.Execute(delete);

      IsDeleted = true;
    }
  }
}
