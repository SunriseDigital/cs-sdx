using System;
using System.Data.Common;

namespace Sdx.Db
{
  public abstract class Factory
  {
    DbProviderFactory factory;

    protected abstract DbProviderFactory GetFactory();

    public string ConnectionString { get; set; }

    public Factory()
    {
      this.factory = this.GetFactory();
    }

    public DbConnection CreateConnection()
    {
      DbConnection con = this.factory.CreateConnection();
      con.ConnectionString = this.ConnectionString;
      return con;
    }

    public DbParameter CreateParameter(string key, string value)
    {
      DbParameter param = this.factory.CreateParameter();
      param.ParameterName = key;
      param.Value = value;

      return param;
    }

    public Where CreateWhere()
    {
      return new Where(this);
    }

    public DbCommand CreateCommand()
    {
      return this.factory.CreateCommand();
    }

    public DbCommandBuilder CreateCommandBuilder()
    {
      return this.factory.CreateCommandBuilder();
    }

    public Select CreateSelect()
    {
      return new Select(this);
    }
  }
}
