using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Sdx.Db.Sql;
using System.Linq;

namespace Sdx.Db
{
  public class Connection : IDisposable
  {
    private bool disposed = false;

    public Adapter Adapter { get; private set; }

    private DbConnection DbConnection { get; set; }

    private DbTransaction DbTransaction { get; set; }

    public string ConnectionString
    {
      get
      {
        return this.DbConnection.ConnectionString;
      }

      set
      {
        this.DbConnection.ConnectionString = value;
      }
    }

    public ConnectionState State
    {
      get
      {
        return this.DbConnection.State;
      }
    }

    public Connection(Adapter adapter)
    {
      this.Adapter = adapter;
      this.DbConnection = this.Adapter.Factory.CreateConnection();
    }

    private void ThrowExceptionIfDisposed()
    {
      if (this.disposed)
      {
        throw new ObjectDisposedException("This object is already disposed.");
      }
    }

    public void Dispose()
    {
      lock(this)
      {
        if(this.disposed)
        {
          return;
        }

        this.disposed = true;

        if (this.DbTransaction != null)
        {
          this.DbTransaction.Dispose();
          this.DbTransaction = null;
        }

        this.Close();
        this.DbConnection.Dispose();
        this.DbConnection = null;

        this.Adapter = null;

        GC.SuppressFinalize(this);
      }
    }

    public void Close()
    {
      Sql.Log log = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        log = Sdx.Context.Current.DbProfiler.Begin("CLOSE");
      }

      this.DbConnection.Close();

      if (log != null)
      {
        log.End();
        log.Adapter = this.Adapter;
      }
    }

    public void Open()
    {
      this.ThrowExceptionIfDisposed();
      Sql.Log log = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        log = Sdx.Context.Current.DbProfiler.Begin("OPEN");
      }

      this.DbConnection.Open();

      if (log != null)
      {
        log.End();
        log.Adapter = this.Adapter;
      }
      
    }

    public DbTransaction BeginTransaction()
    {
      this.ThrowExceptionIfDisposed();

      Sql.Log log = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        log = Sdx.Context.Current.DbProfiler.Begin("BEGIN TRANSACTION");
      }

      this.DbTransaction = this.DbConnection.BeginTransaction();

      if (log != null)
      {
        log.End();
        log.Adapter = this.Adapter;
      }

      return this.DbTransaction;
    }

    public void Commit()
    {
      this.ThrowExceptionIfDisposed();
      if (this.DbTransaction == null)
      {
        throw new InvalidOperationException("Missing DbTransaction");
      }

      Sql.Log log = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        log = Sdx.Context.Current.DbProfiler.Begin("COMMIT");
      }

      this.DbTransaction.Commit();

      if (log != null)
      {
        log.End();
        log.Adapter = this.Adapter;
      }
    }

    public void Rollback()
    {
      this.ThrowExceptionIfDisposed();
      if (this.DbTransaction == null)
      {
        throw new InvalidOperationException("Missing DbTransaction");
      }

      Sql.Log log = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        log = Sdx.Context.Current.DbProfiler.Begin("ROLLBACK");
      }

      this.DbTransaction.Rollback();

      if (log != null)
      {
        log.End();
        log.Adapter = this.Adapter;
      }
    }

    public DbCommand CreateCommand()
    {
      this.ThrowExceptionIfDisposed();
      var command = this.Adapter.CreateCommand();
      command.Connection = this.DbConnection;
      command.Transaction = this.DbTransaction;
      return command;
    }

    public object FetchLastInsertId()
    {
      this.ThrowExceptionIfDisposed();
      return this.Adapter.FetchLastInsertId(this);
    }

    private T ExecuteCommand<T>(DbCommand command, string comment, Func<T> func)
    {
      this.ThrowExceptionIfDisposed();
      command.Connection = this.DbConnection;
      command.Transaction = this.DbTransaction;

      Sql.Log log = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        log = Sdx.Context.Current.DbProfiler.Begin(command);
      }

      T result = default(T);
      try
      {
        result = func();
      }
      catch (Exception e)
      {
        throw new Sdx.Db.DbException(e.Message + "\n" + command.CommandText);
      }

      if (log != null)
      {
        log.End();
        log.Adapter = this.Adapter;
        log.Comment = comment;
      }

      return result;
    }

    public int Execute(INonQueryBuilder builder)
    {
      using (var command = builder.Build())
      {
        return this.ExecuteNonQuery(command);
      }
    }

    public object ExecuteScalar(DbCommand command)
    {
      return this.ExecuteCommand<object>(command, null, () => {
        return command.ExecuteScalar();
      });
    }

    public int ExecuteNonQuery(DbCommand command)
    {
      return this.ExecuteCommand<int>(command, null, () => {
        return command.ExecuteNonQuery();
      });
    }

    public DbDataReader ExecuteReader(DbCommand command)
    {
      string comment = null;
      if (command.Parameters.Count > 0)
      {
        //Select.SetCommentのため、最後がコメントかチェックする
        var lastIndex = command.Parameters.Count - 1;
        var param = command.Parameters[lastIndex];
        if (param.ParameterName == Sql.Select.CommentParameterKey)
        {
          command.Parameters.RemoveAt(lastIndex);
          comment = param.Value.ToString();
        }
      }

      return this.ExecuteCommand<DbDataReader>(command, comment, () => {
        return command.ExecuteReader();
      });
    }

    private T Fetch<T>(DbCommand command, Func<DbDataReader, T> func)
    {
      this.ThrowExceptionIfDisposed();
      command.Connection = this.DbConnection;
      command.Transaction = this.DbTransaction;
      using (var reader = this.ExecuteReader(command))
      {
        return func(reader);
      }
    }

    public List<Dictionary<string, T>> FetchDictionaryList<T>(DbCommand command)
    {
      return this.Fetch<List<Dictionary<string, T>>>(command, (reader) => {
        var list = new List<Dictionary<string, T>>();

        while (reader.Read())
        {
          var row = new Dictionary<string, T>();
          for (var i = 0; i < reader.FieldCount; i++)
          {
            row[reader.GetName(i)] = this.castDbValue<T>(reader.GetValue(i));
          }

          list.Add(row);
        }

        return list;
      });
    }

    public List<KeyValuePair<TKey, TValue>> FetchKeyValuePairList<TKey, TValue>(DbCommand command)
    {
      return this.Fetch<List<KeyValuePair<TKey, TValue>>>(command, (reader) => {
        var list = new List<KeyValuePair<TKey, TValue>>();
        while (reader.Read())
        {
          var row = new KeyValuePair<TKey, TValue>(
            this.castDbValue<TKey>(reader.GetValue(0)),
            this.castDbValue<TValue>(reader.GetValue(1))
          );

          list.Add(row);
        }
        return list;
      });
    }

    public List<T> FetchList<T>(DbCommand command)
    {
      return this.Fetch<List<T>>(command, (reader) => {
        var list = new List<T>();
        while (reader.Read())
        {
          list.Add(this.castDbValue<T>(reader.GetValue(0)));
        }
        return list;
      });
    }

    public T FetchOne<T>(DbCommand command)
    {
      return this.Fetch<T>(command, (reader) => {
        while (reader.Read())
        {
          return this.castDbValue<T>(reader.GetValue(0));
        }

        return default(T);
      });
    }

    public Dictionary<string, T> FetchDictionary<T>(DbCommand command)
    {
      return this.Fetch<Dictionary<string, T>>(command, (reader) => {
        var dic = new Dictionary<string, T>();
        while (reader.Read())
        {
          for (var i = 0; i < reader.FieldCount; i++)
          {
            dic[reader.GetName(i)] = this.castDbValue<T>(reader.GetValue(i));
          }

          break;
        }

        return dic;
      });
    }

    public bool IsAttachedTo(DbCommand command)
    {
      return command.Connection == this.DbConnection && command.Transaction == this.DbTransaction;
    }

    public Dictionary<string, T> FetchDictionary<T>(Sql.Select select)
    {
      Dictionary<string, T> result;
      using (var command = select.Build())
      {
        result = this.FetchDictionary<T>(command);
      }

      return result;
    }

    public T FetchOne<T>(Sql.Select select)
    {
      T result = default(T);
      using (var command = select.Build())
      {
        result = this.FetchOne<T>(command);
      }

      return result;
    }

    public List<T> FetchList<T>(Sql.Select select)
    {
      List<T> result = null;
      using (var command = select.Build())
      {
        result = this.FetchList<T>(command);
      }

      return result;
    }

    public List<KeyValuePair<TKey, TValue>> FetchKeyValuePairList<TKey, TValue>(Sql.Select select)
    {
      List<KeyValuePair<TKey, TValue>> result = null;
      using (var command = select.Build())
      {
        result = this.FetchKeyValuePairList<TKey, TValue>(command);
      }

      return result;
    }

    public List<Dictionary<string, T>> FetchDictionaryList<T>(Sql.Select select)
    {
      List<Dictionary<string, T>> result = null;
      using (var command = select.Build())
      {
        result = this.FetchDictionaryList<T>(command);
      }

      return result;
    }

    public T FetchRecord<T>(Sql.Select select) where T : Record, new()
    {
      var resultSet = this.FetchRecordSet<T>(select);

      if (resultSet.Count == 0)
      {
        return null;
      }

      return resultSet[0];
    }

    /// <summary>
    /// SQLを実行しRecordSetを生成して返します。
    /// </summary>
    /// <typeparam name="T">Recordのクラスを指定</typeparam>
    /// <param name="contextName">
    /// １対多のJOINを行うと行数が「多」の行数になるが、指定したテーブル（エイリアス）名の主キーの値に基づいて一つのレコードにまとめます。
    /// 省略した場合、指定したRecordクラスのMetaからテーブル名を使用します。
    /// </param>
    /// <returns></returns>
    public RecordSet<T> FetchRecordSet<T>(Sql.Select select) where T : Record, new()
    {
      var prop = typeof(T).GetProperty("Meta");
      if (prop == null)
      {
        throw new NotImplementedException("Missing Meta property in " + typeof(T));
      }

      var meta = prop.GetValue(null, null) as TableMeta;
      if (meta == null)
      {
        throw new NotImplementedException("Initialize TableMeta for " + typeof(T));
      }

      RecordSet<T> recordSet = null;
      using (var command = select.Build())
      {
        recordSet = this.Fetch<RecordSet<T>>(command, (reader) =>
        {
          var resultSet = new RecordSet<T>();
          resultSet.Build(reader, select, meta.Name);
          return resultSet;
        });
      }

      return recordSet;
    }

    /// <summary>
    /// RecordSetの基点となるテーブルは最初にAddFromされた<see cref="Context" />です。
    /// </summary>
    /// <param name="select"></param>
    /// <returns></returns>
    public RecordSet<Record> FetchRecordSet(Select select)
    {
      var firstFrom = select.ContextList.First((kv) => kv.Value.JoinType == JoinType.From).Value;
      var meta = firstFrom.Table.OwnMeta;

      RecordSet<Record> recordSet = null;
      using (var command = select.Build())
      {
        recordSet = this.Fetch<RecordSet<Record>>(command, (reader) =>
        {
          var resultSet = new RecordSet<Record>();
          resultSet.Build(reader, select, meta.Name);
          return resultSet;
        });
      }

      return recordSet;
    }

    private T castDbValue<T>(object value)
    {
      if (typeof(T) == typeof(string))
      {
        return (T)(object)value.ToString();
      }
      else
      {
        return (T)value;
      }
    }

    public void Save(Record record)
    {
      if (record.IsDeleted)
      {
        throw new InvalidOperationException("This record is already deleted.");
      }

      if (record.UpdatedValues.Count == 0)
      {
        return;
      }

      if (record.IsNew)
      {
        var insert = this.Adapter.CreateInsert();
        insert.SetInto(record.OwnMeta.Name);

        foreach (var columnValue in record.UpdatedValues)
        {
          insert.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        this.Execute(insert);

        //値を保存後も取得できるようにする
        var newValues = new Dictionary<string, object>();
        foreach (var columnValue in record.UpdatedValues)
        {
          var key = Record.BuildColumnAliasWithContextName(columnValue.Key, record.ContextName);
          newValues[key] = columnValue.Value;
        }

        //保存に成功し、PkeyがNullだったらAutoincrementのはず。
        //Autoincrementは通常テーブルに１つしか作れないはず（MySQLとSQLServerはそうだった）
        var pkey = record.OwnMeta.Pkeys[0];
        var pkeyValue = record.GetValue(pkey);
        if (pkeyValue == null)
        {
          var key = Record.BuildColumnAliasWithContextName(pkey, record.ContextName);
          newValues[key] = this.FetchLastInsertId();
        }

        record.ValuesList.Add(newValues);
      }
      else
      {
        var update = this.Adapter.CreateUpdate();
        update.SetTable(record.OwnMeta.Name);
        foreach (var columnValue in record.UpdatedValues)
        {
          update.AddColumnValue(columnValue.Key, columnValue.Value);
        }

        record.AppendPkeyWhere(update.Where);

        this.Execute(update);

        //値を保存後も取得できるようにする
        foreach (var row in record.ValuesList)
        {
          foreach (var columnValue in record.UpdatedValues)
          {
            var key = Record.BuildColumnAliasWithContextName(columnValue.Key, record.ContextName);
            row[key] = columnValue.Value;
          }
        }
      }

      record.UpdatedValues.Clear();
    }

    public void Delete(Record record)
    {
      if (record.IsNew)
      {
        return;
      }

      var delete = this.Adapter.CreateDelete();
      delete.From = record.OwnMeta.Name;
      record.AppendPkeyWhere(delete.Where);

      this.Execute(delete);

      record.IsDeleted = true;
    }
  }
}
