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
        if(!Sdx.Context.HasSdxHttpModule)
        {
          throw new Exception("Require Sdx.Web.HttpModule for SharedConnection");
        }

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
    public Connection UseConnection(DbConnection conn)
    {
      if (conn.ConnectionString != ConnectionString)
      {
        throw new InvalidOperationException("Not match connection string " + conn.ConnectionString + " and " + ConnectionString);
      }

      if (conn.State != ConnectionState.Open)
      {
        conn.Open();
      }

      return new Connection(this, conn);
    }

    /// <summary>
    /// これで生成した接続は閉じないので、Dispose忘れじゃないかと勘違いするので名前をuseにしました。
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    [Obsolete("UseConnectionを使用して下さい")]
    public Connection CreateConnection(DbConnection conn)
    {
      return UseConnection(conn);
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

    public DbCommand CreateCommand(string sql = null)
    {
      var command = this.Factory.CreateCommand();
      command.CommandText = sql;
      return command;
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
        var str = obj as string;
        //スキーマー名を含めたいときテーブル名に`.`で含めることが可能です。テーブル名に`.`が含まれる場合は`..`でエスケープ可能。
        var mword = "%%SDX_DOT_REPLACE_ESCAPE%%";
        var escapedChunk = str.Replace("..", mword).Split('.').Select(w => builder.QuoteIdentifier(w));
        return string.Join(".", escapedChunk).Replace(mword, ".");
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

    abstract internal IEnumerable<string> FetchTableNames(Connection conn);

    public List<Table.Column> FetchColumns(string tableName)
    {
      List<Table.Column> columns;
      using(var conn = CreateConnection())
      {
        conn.Open();
        columns = FetchColumns(tableName, conn);
      }

      return columns;
    }

    abstract internal List<Table.Column> FetchColumns(string tableName, Connection conn);
  }
}
