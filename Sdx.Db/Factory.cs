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

    public Sdx.Db.Query.Where CreateWhere()
    {
      return new Sdx.Db.Query.Where(this);
    }

    public DbCommand CreateCommand()
    {
      return this.factory.CreateCommand();
    }

    public DbCommandBuilder CreateCommandBuilder()
    {
      return this.factory.CreateCommandBuilder();
    }

    public Sdx.Db.Query.Select CreateSelect()
    {
      return new Sdx.Db.Query.Select(this);
    }

    public DbDataAdapter CreateDataAdapter()
    {
      return this.factory.CreateDataAdapter();
    }

    public string QuoteIdentifier(string unquotedIdentifier)
    {
      return this.builder.QuoteIdentifier(unquotedIdentifier);
    }

    public string QuoteIdentifier(Sdx.Db.Query.Expr expr)
    {
      return expr.ToString();
    }

    public string QuoteIdentifier(Sdx.Db.Query.Table table)
    {
      return this.QuoteIdentifier(table.Target);
    }

    private string QuoteIdentifier(object obj)
    {
      if (obj is Sdx.Db.Query.Expr)
      {
        return this.QuoteIdentifier(obj as Sdx.Db.Query.Expr);
      }
      else
      {
        return this.QuoteIdentifier(obj as string);
      }
    }
  }
}
