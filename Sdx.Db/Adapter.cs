﻿using System;
using System.Data.Common;
using System.Collections.Generic;

namespace Sdx.Db
{
  public abstract class Adapter
  {
    private DbProviderFactory factory;
    private DbCommandBuilder builder;

    protected abstract DbProviderFactory GetFactory();

    public string ConnectionString { get; set; }


    public Adapter()
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

    public Query.Condition CreateCondition(string baseCond = null)
    {
      return new Query.Condition(baseCond);
    }

    public string QuoteIdentifier(object obj)
    {
      if (obj is Sdx.Db.Query.Expr)
      {
        return (obj as Sdx.Db.Query.Expr).ToString();
      }
      else if(obj is string)
      {
        return this.builder.QuoteIdentifier(obj as string);
      }
      else
      {
        throw new Exception("QuoteIdentifier support only Sdx.Db.Query.Expr or string, "+obj.GetType()+" given.");
      }
    }

    internal abstract string AppendLimitQuery(string selectSql, int limit, int offset);
  }
}
