using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public class Connection : IDisposable
  {
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

    public System.Data.ConnectionState State
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

    public void Dispose()
    {
      if (this.DbTransaction != null)
      {
        this.DbTransaction.Dispose();
      }

      this.DbConnection.Dispose();

      Console.WriteLine("Connection.Dispose");
    }

    public void Open()
    {
      this.DbConnection.Open();
    }

    public DbTransaction BeginTransaction()
    {
      this.DbTransaction = this.DbConnection.BeginTransaction();
      return this.DbTransaction;
    }

    public void Commit()
    {
      if(this.DbTransaction == null)
      {
        throw new InvalidOperationException("Missing DbTransaction");
      }
      this.DbTransaction.Commit();
    }

    public void Rollback()
    {
      if (this.DbTransaction == null)
      {
        throw new InvalidOperationException("Missing DbTransaction");
      }
      this.DbTransaction.Rollback();
    }
  }
}
