using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using Sdx.Db.Sql;
using System.Data;

namespace Sdx.Db.Adapter
{
  public abstract class Base
  {
    internal DbProviderFactory Factory { get; private set; }
    private DbCommandBuilder builder;
    protected const string PasswordForSecureConnectionString = "******";
    internal const string SharedConnectionKey = "Sdx.Db.Adapter.Base.SharedConnectionKey";

    protected abstract DbProviderFactory GetFactory();

    /// <summary>
    /// ToStringで返される文字列。本来一般ユーザーが目にするものではないが不用意に表示される可能性もあるので、パスワード部分をわからないように置換するのが望ましい。
    /// パスワードはPWD_FOR_SECURE_CONNECTION_STRINGで置換してください。
    /// </summary>
    protected abstract string SecureConnectionString { get; }

    public string ConnectionString { get; set; }

    public Base()
    {
      this.Factory = this.GetFactory();
      this.builder = this.Factory.CreateCommandBuilder();
    }

    private string UniqueDbString
    {
      get
      {
        return  ConnectionString + "@" + GetType().FullName;
      }
    }

    public Connection SharedConnection
    {
      get
      {
        if(!Sdx.Context.Current.Vars.ContainsKey(SharedConnectionKey))
        {
          Sdx.Context.Current.Vars[SharedConnectionKey] = new Dictionary<string, Connection>();
        }

        var dic = Sdx.Context.Current.Vars.As<Dictionary<string, Connection>>(SharedConnectionKey);

        if(!dic.ContainsKey(UniqueDbString))
        {
          dic[UniqueDbString] = CreateConnection();
          dic[UniqueDbString].Open();
        }

        return dic[UniqueDbString];
      }
    }

    public Connection CreateConnection()
    {
      var con = new Connection(this);
      con.ConnectionString = this.ConnectionString;
      return con;
    }

    /// <summary>
    /// 既に生成されているDbConnectionを使用してSdx.Db.Connectionを生成します。
    /// 渡されたDbConnectionが開いていなかった場合、中でOpenします。
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public Connection CreateConnection(DbConnection conn)
    {
      if(conn.ConnectionString != ConnectionString)
      {
        throw new InvalidOperationException("Not match connection string " + conn.ConnectionString + " and " + ConnectionString);
      }

      if (conn.State != ConnectionState.Open)
      {				
        conn.Open();
      }

      return new Connection(this, conn);
    }

    public DbParameter CreateParameter(string key, object value)
    {
      DbParameter param = this.Factory.CreateParameter();
      param.ParameterName = key;
      param.Value = value;

      return param;
    }

    internal abstract void InitSelectEvent(Select select);

    internal abstract object FetchLastInsertId(Connection connection);

    public DbCommand CreateCommand()
    {
      return this.Factory.CreateCommand();
    }

    public DbCommandBuilder CreateCommandBuilder()
    {
      return this.Factory.CreateCommandBuilder();
    }

    public DbDataAdapter CreateDataAdapter()
    {
      return this.Factory.CreateDataAdapter();
    }

    public Sql.Condition CreateCondition()
    {
      return new Sql.Condition();
    }

    public string QuoteIdentifier(object obj)
    {
      if (obj is Sql.Expr)
      {
        return (obj as Sql.Expr).ToString();
      }
      else if(obj is string)
      {
        return this.builder.QuoteIdentifier(obj as string);
      }
      else
      {
        throw new NotSupportedException("QuoteIdentifier support only Query.Expr or string, "+obj.GetType()+" given.");
      }
    }

    public override string ToString()
    {
      var prefix = this.GetType().Name + ": ";
      if(this.ConnectionString == null)
      {
        return prefix;
      }

      return prefix + SecureConnectionString;
    }

    public Sql.Select CreateSelect()
    {
      return new Sql.Select(this);
    }

    public Sql.Insert CreateInsert()
    {
      return new Sql.Insert(this);
    }

    public Sql.Update CreateUpdate()
    {
      return new Sql.Update(this);
    }

    public Sql.Delete CreateDelete()
    {
      return new Sql.Delete(this);
    }

    public abstract string RandomOrderKeyword { get; }

    public string QuoteColumn(string contextName, string column)
    {
      return string.Format(
        "{0}.{1}",
        QuoteIdentifier(contextName),
        QuoteIdentifier(column)
      );
    }
  }
}
