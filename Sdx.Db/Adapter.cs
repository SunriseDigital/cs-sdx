using System;
using System.Data.Common;

namespace Sdx.Db
{
  public abstract class Adapter : IDisposable
  {
    private DbConnection connection;

    public Adapter()
    {
      this.connection = this.CreateDbConection();
    }

    public String ConnectionString
    {
      get
      {
        return this.connection.ConnectionString;
      }

      set
      {
        this.connection.ConnectionString = value;
      }
    }

    protected abstract DbConnection CreateDbConection();

    public void Open()
    {
      this.connection.Open();
    }

    public DbParameter CreateParameter(string key, string value)
    {
      return this.CreateDbParameter(key, value);
    }

    protected abstract DbParameter CreateDbParameter(string key, string value);

    public DbTransaction BeginTransaction()
    {
      return this.connection.BeginTransaction();
    }

    public DbCommand CreateCommand()
    {
      return this.connection.CreateCommand();
    }

    public void Dispose()
    {
      this.connection.Dispose();
    }
  }
}
