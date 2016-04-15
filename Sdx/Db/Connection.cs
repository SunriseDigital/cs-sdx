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

    public Connection(Adapter.Base adapter)
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

    internal T Fetch<T>(DbCommand command, Func<DbDataReader, T> func)
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
  }
}
