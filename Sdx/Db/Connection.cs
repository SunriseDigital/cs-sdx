using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Sdx.Db
{
  public class Connection : IDisposable
  {
    private bool disposed = false;

    public Adapter Adapter { get; private set; }

    public DbConnection DbConnection { get; private set; }

    public DbTransaction DbTransaction { get; internal set; }

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

        this.DbConnection.Dispose();
        this.DbConnection = null;

        this.Adapter = null;

        GC.SuppressFinalize(this);
      }
    }

    public void Open()
    {
      this.ThrowExceptionIfDisposed();
      this.DbConnection.Open();
    }

    public DbTransaction BeginTransaction()
    {
      this.ThrowExceptionIfDisposed();
      this.DbTransaction = this.DbConnection.BeginTransaction();
      return this.DbTransaction;
    }

    public void Commit()
    {
      this.ThrowExceptionIfDisposed();
      if (this.DbTransaction == null)
      {
        throw new InvalidOperationException("Missing DbTransaction");
      }
      this.DbTransaction.Commit();
    }

    public void Rollback()
    {
      this.ThrowExceptionIfDisposed();
      if (this.DbTransaction == null)
      {
        throw new InvalidOperationException("Missing DbTransaction");
      }
      this.DbTransaction.Rollback();
    }

    public void Insert(string tableName, Dictionary<string, object> values)
    {
      using (var command = this.Adapter.CreateCommand())
      {
        command.Connection = this.DbConnection;
        command.Transaction = this.DbTransaction;

        var insertBuilder = new StringBuilder();
        insertBuilder.AppendFormat("INSERT INTO " + this.Adapter.QuoteIdentifier(tableName) + " (");

        var valuesBuilder = new StringBuilder();
        valuesBuilder.Append(" VALUES (");

        var count = values.Count;
        foreach(var kv in values)
        {
          var parameterName = "@" + (values.Count - count);
          insertBuilder.Append(this.Adapter.QuoteIdentifier(kv.Key));
          valuesBuilder.Append(parameterName);

          var param = command.CreateParameter();
          param.ParameterName = parameterName;
          param.Value = kv.Value;
          command.Parameters.Add(param);

          --count;
          if(count > 0)
          {
            insertBuilder.Append(", ");
            valuesBuilder.Append(", ");
          }
        }

        insertBuilder.Append(")");
        valuesBuilder.Append(")");

        command.CommandText = insertBuilder.ToString() + valuesBuilder.ToString();

        var rowCount = this.Adapter.ExecuteNonQuery(command);
        if(rowCount != 1)
        {
          throw new DbException("Illegal row count " + rowCount);
        }
      }
    }

    public DbCommand CreateCommand()
    {
      var command = this.Adapter.CreateCommand();
      command.Connection = this.DbConnection;
      command.Transaction = this.DbTransaction;
      return command;
    }

    public ulong FetchLastInsertId()
    {
      return this.Adapter.FetchLastInsertId(this);
    }
  }
}
