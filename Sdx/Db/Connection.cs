using System;
using System.Data;
using System.Data.Common;

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
  }
}
