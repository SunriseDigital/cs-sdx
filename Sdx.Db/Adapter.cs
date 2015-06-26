using System;
using System.Data.Common;

namespace Sdx.Db
{
  public abstract class Adapter
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

    public DbConnection Connection
    {
      get
      {
        return this.connection;
      }
    }

    public DbParameter CreateParameter(string key, string value)
    {
      return this.CreateDbParameter(key, value);
    }

    protected abstract DbParameter CreateDbParameter(string key, string value);
  }
}
