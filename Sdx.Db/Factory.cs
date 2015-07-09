using System;
using System.Data.Common;

namespace Sdx.Db
{
  public abstract class Factory
  {
    private DbProviderFactory factory;
    private DbCommandBuilder builder;

    protected abstract DbProviderFactory GetFactory();

    public string ConnectionString { get; set; }

    public Factory()
    {
      this.factory = this.GetFactory();
      this.builder = this.factory.CreateCommandBuilder();
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

    public DbDataAdapter CreateDataAdapter()
    {
      return this.factory.CreateDataAdapter();
    }

    public string QuoteIdentifier(string unquotedIdentifier)
    {
      return this.builder.QuoteIdentifier(unquotedIdentifier);
    }

    public string QuoteIdentifier(Expr expr)
    {
      return expr.ToString();
    }

    internal string QuoteIdentifier(SelectColumn column)
    {
      if(column.isExpr())
      {
        return this.QuoteIdentifier(column.Name as Expr);
      }
      else
      {
        return this.QuoteIdentifier(column.Name as string);
      }
    }
  }
}
