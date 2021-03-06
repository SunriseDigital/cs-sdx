﻿using System;
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

    public Adapter.Base Adapter { get; private set; }

    public DbConnection DbConnection { get; private set; }

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

    public Connection(Adapter.Base adapter)
    {
      this.Adapter = adapter;
      this.DbConnection = this.Adapter.Factory.CreateConnection();
    }

    public Connection(Adapter.Base adapter, DbConnection conn)
    {
      this.Adapter = adapter;
      this.DbConnection = conn;
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
        throw new Sdx.Db.DbException(e.Message + "\n" + command.CommandText, e);
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

    public List<Dictionary<string, object>> FetchDictionaryList(DbCommand command)
    {
      return FetchDictionaryList<object>(command);
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

    public List<KeyValuePair<object, object>> FetchKeyValuePairList(DbCommand command)
    {
      return FetchKeyValuePairList<object, object>(command);
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

    public List<object> FetchList(DbCommand command)
    {
      return FetchList<object>(command);
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

    public object FetchOne(DbCommand command)
    {
      return FetchOne<object>(command);
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

    public Dictionary<string, object> FetchDictionary(DbCommand command)
    {
      return FetchDictionary<object>(command);
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

    public Dictionary<string, object> FetchDictionary(Sql.Select select)
    {
      return FetchDictionary<object>(select);
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

    public object FetchOne(Sql.Select select)
    {
      return FetchOne<object>(select);
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

    public List<object> FetchList(Sql.Select select)
    {
      return FetchList<object>(select);
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

    public List<KeyValuePair<object, object>> FetchKeyValuePairList(Sql.Select select)
    {
      return FetchKeyValuePairList<object, object>(select);
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

    public List<Dictionary<string, object>> FetchDictionaryList(Sql.Select select)
    {
      return FetchDictionaryList<object>(select);
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

    public Record FetchRecord(Sql.Select select)
    {
      return FetchRecord<Record>(select);
    }

    public T FetchRecord<T>(Sql.Select select) where T : Sdx.Db.Record
    {
      var resultSet = this.FetchRecordSet(select);

      if (resultSet.Count == 0)
      {
        return null;
      }

      return (T)resultSet[0];
    }

    public Record FetchRecord(Sql.Select select, string contextName)
    {
      return FetchRecord<Record>(select, contextName);
    }

    public T FetchRecord<T>(Sql.Select select, string contextName) where T : Sdx.Db.Record
    {
      var resultSet = this.FetchRecordSet(select, contextName);

      if (resultSet.Count == 0)
      {
        return null;
      }

      return (T)resultSet[0];
    }

    /// <summary>
    /// SQLを実行しRecordSetを生成して返します。
    /// </summary>
    /// <typeparam name="T">Recordのクラスを指定</typeparam>
    /// TODO ↓このコメントは古い？ただ、このオプションが無いと同じテーブルを二つJOINした時にまとめられないのでは？調査する。
    /// <param name="contextName">
    /// １対多のJOINを行うと行数が「多」の行数になるが、指定したテーブル（エイリアス）名の主キーの値に基づいて一つのレコードにまとめます。
    /// 省略した場合、指定したRecordクラスのMetaからテーブル名を使用します。
    /// </param>
    /// <returns></returns>
    public RecordSet FetchRecordSet(Sql.Select select)
    {
      var firstFrom = select.ContextList.FirstOrDefault((kv) => kv.Value.JoinType == JoinType.From).Value;
      if (firstFrom == null)
      {
        throw new InvalidOperationException("Missing from clause in table list [" + select.Contexts.Select(ctx => ctx.Name).Aggregate((prev, next) => prev + "," + next) + "]");
      }
      return FetchRecordSet(select, firstFrom.Name);
    }

    public RecordSet FetchRecordSet(Select select, string contextName)
    {
      select.Connection = this;
      RecordSet recordSet = null;
      using (var command = select.Build())
      {
        recordSet = this.Fetch<RecordSet>(command, (reader) =>
        {
          var resultSet = new RecordSet();
          resultSet.Build(reader, select, contextName);
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

    public int CountRow(Select select)
    {
      Select clonedSel = null;

      if (select.GroupList.Any())
      {
        clonedSel = select.Adapter.CreateSelect();
        clonedSel.AddColumn(Sdx.Db.Sql.Expr.Wrap("COUNT(*)"));

        var subsel = (Select)select.Clone();
        subsel.OrderList.Clear();
        clonedSel.AddFrom(subsel, "_t");
      }
      else
      {
        clonedSel = (Select)select.Clone();
        clonedSel.ClearColumns().AddColumn(Sdx.Db.Sql.Expr.Wrap("COUNT(*)"));
        clonedSel.OrderList.Clear();
      }

      //DBベンダーによって帰ってくる型がintだったりlongだったりします。
      //いちいち型を識別してキャストするの面倒だったのでこんな感じに。
      var count = FetchOne<string>(clonedSel);

      return Int32.Parse(count);
    }

    public IEnumerable<string> FetchTableNames()
    {
      return Adapter.FetchTableNames(this);
    }

    public DataTable GetSchema(string collectionName)
    {
      return DbConnection.GetSchema(collectionName);
    }

    public DataTable GetSchema()
    {
      return DbConnection.GetSchema();
    }

    public DataTable GetSchema(string collectionName, string[] restrictionValues)
    {
      return DbConnection.GetSchema(collectionName, restrictionValues);
    }

    public List<Table.Column> FetchColumns(string tableName)
    {
      return Adapter.FetchColumns(tableName, this);
    }

    public string Database
    {
      get
      {
        return DbConnection.Database;
      }
    }

    public string DataSource
    {
      get
      {
        return DbConnection.DataSource;
      }
    }

    public void UseTransaction(DbTransaction transaction)
    {
      DbTransaction = transaction;
    }
  }
}
