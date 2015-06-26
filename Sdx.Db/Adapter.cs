using System;
using System.Data.Common;

namespace Sdx.Db
{
  public abstract class Adapter
  {
    private DbConnection connection;

    public Adapter()
    {
      this.connection = this.createDbConection();
    }

    protected abstract DbConnection createDbConection();

    public DbCommand CreateCommand()
    {
      return this.connection.CreateCommand();
    }

    public DbParameter CreateParameter(string key, string value)
    {
      return this.CreateDbParameter(key, value);
    }

    protected abstract DbParameter CreateDbParameter(string key, string value);
  }
}
