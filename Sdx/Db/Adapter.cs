using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using Sdx.Db.Query;

namespace Sdx.Db
{
  public abstract class Adapter
  {
    private DbProviderFactory factory;
    private DbCommandBuilder builder;
    public const string PWD_FOR_SECURE_CONNECTION_STRING = "******";

    protected abstract DbProviderFactory GetFactory();

    /// <summary>
    /// ToStringで返される文字列。本来一般ユーザーが目にするものではないが不用意に表示される可能性もあるので、パスワード部分をわからないように置換するのが望ましい。
    /// パスワードはPWD_FOR_SECURE_CONNECTION_STRINGで置換してください。
    /// </summary>
    protected abstract string SecureConnectionString { get; }

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

    public DbDataAdapter CreateDataAdapter()
    {
      return this.factory.CreateDataAdapter();
    }

    public DbDataReader ExecuteReader(DbCommand command)
    {
      Query.Log query = null;
      string comment = null;
      if(command.Parameters.Count > 0)
      {
        //最後がコメントかチェックする
        var lastIndex = command.Parameters.Count - 1;
        var param = command.Parameters[lastIndex];
        if (param.ParameterName == Query.Select.CommentParameterKey)
        {
          command.Parameters.RemoveAt(lastIndex);
          comment = param.Value.ToString();
        }
      }

      if (Sdx.Context.Current.DbProfiler != null)
      {
        query = Sdx.Context.Current.DbProfiler.Begin(command);
      }

      DbDataReader reader = null;
      try
      {
        reader = command.ExecuteReader();
      }
      catch(Exception e)
      {
        throw new Sdx.Db.DbException(e.Message + "\n" + command.CommandText);
      }

      if (query != null)
      {
        query.End();
        query.Adapter = this;
        query.Comment = comment;
      }

      return reader;
    }

    public Query.Condition CreateCondition()
    {
      return new Query.Condition();
    }

    public string QuoteIdentifier(object obj)
    {
      if (obj is Query.Expr)
      {
        return (obj as Query.Expr).ToString();
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

    internal abstract string AppendLimitQuery(string selectSql, int limit, int offset);

    public T FetchRecord<T>(Query.Select select, DbConnection connection = null) where T : Record, new()
    {
      var resultSet = this.FetchRecordSet<T>(select, connection);

      if (resultSet.Count == 0)
      {
        return null;
      }

      return resultSet[0];
    }

    /// <summary>
    /// SQLを実行しRecordSetを生成して返します。
    /// </summary>
    /// <typeparam name="T">Recordのクラスを指定</typeparam>
    /// <param name="contextName">
    /// １対多のJOINを行うと行数が「多」の行数になるが、指定したテーブル（エイリアス）名の主キーの値に基づいて一つのレコードにまとめます。
    /// 省略した場合、指定したRecordクラスのMetaからテーブル名を使用します。
    /// </param>
    /// <returns></returns>
    public RecordSet<T> FetchRecordSet<T>(Query.Select select, DbConnection connection = null) where T : Record, new()
    {
      select.Adapter = this;

      var prop = typeof(T).GetProperty("Meta");
      if (prop == null)
      {
        throw new NotImplementedException("Missing Meta property in " + typeof(T));
      }

      var meta = prop.GetValue(null, null) as TableMeta;
      if (meta == null)
      {
        throw new NotImplementedException("Initialize TableMeta for " + typeof(T));
      }

      var contextName = meta.Name;

      var command = select.Build();
      command.Connection = connection;

      var resultSet = new RecordSet<T>();

      Action action = () => {
        var reader = this.ExecuteReader(command);
        resultSet.Build(reader, select, contextName);
      };

      if (command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          action();
        }
      }
      else
      {
        action();
      }

      return resultSet;
    }

    public List<Dictionary<string, T>> FetchDictionaryList<T>(Query.Select select, DbConnection connection = null)
    {
      select.Adapter = this;
      var command = select.Build();
      command.Connection = connection;
      return this.FetchDictionaryList<T>(command);
    }

    public List<Dictionary<string, T>> FetchDictionaryList<T>(DbCommand command)
    {
      var list = new List<Dictionary<string, T>>();

      Action action = () => {
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          var row = new Dictionary<string, T>();
          for (var i = 0; i < reader.FieldCount; i++)
          {
            row[reader.GetName(i)] = this.castDbValue<T>(reader.GetValue(i));
          }

          list.Add(row);
        }
      };

      if (command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          action();
        }
      }
      else
      {
        action();
      }

      return list;
    }

    public List<KeyValuePair<TKey, TValue>> FetchKeyValuePairList<TKey, TValue>(Query.Select select, DbConnection connection = null)
    {
      select.Adapter = this;
      var command = select.Build();
      command.Connection = connection;
      return this.FetchKeyValuePairList<TKey, TValue>(command);
    }

    public List<KeyValuePair<TKey, TValue>> FetchKeyValuePairList<TKey, TValue>(DbCommand command)
    {
      var list = new List<KeyValuePair<TKey, TValue>>();

      Action action = () => {
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          var row = new KeyValuePair<TKey, TValue>(
            this.castDbValue<TKey>(reader.GetValue(0)),
            this.castDbValue<TValue>(reader.GetValue(1))
          );

          list.Add(row);
        }
      };

      if (command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          action();
        }
      }
      else
      {
        action();
      }

      return list;
    }

    public List<T> FetchList<T>(Query.Select select, DbConnection connection = null)
    {
      select.Adapter = this;
      var command = select.Build();
      command.Connection = connection;
      return this.FetchList<T>(command);
    }

    public List<T> FetchList<T>(DbCommand command)
    {
      var list = new List<T>();
      Action action = () => {
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          list.Add(this.castDbValue<T>(reader.GetValue(0)));
        }
      };

      if(command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          action();
        }
      }
      else
      {
        action();
      }

      return list;
    }

    public T FetchOne<T>(Query.Select select, DbConnection connection = null)
    {
      select.Adapter = this;
      var command = select.Build();
      command.Connection = connection;
      return this.FetchOne<T>(command);
    }

    public T FetchOne<T>(DbCommand command)
    {
      Func<T> func = () => {
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          return this.castDbValue<T>(reader.GetValue(0));
        }
        return default(T);
      };

      if(command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          return func();
        }
      }
      else
      {
        return func();
      }
    }

    public Dictionary<string, T> FetchDictionary<T>(Query.Select select, DbConnection connection = null)
    {
      select.Adapter = this;
      var command = select.Build();
      command.Connection = connection;
      return this.FetchDictionary<T>(command);
    }

    public Dictionary<string, T> FetchDictionary<T>(DbCommand command)
    {
      var dic = new Dictionary<string, T>();
      Action action = () => {
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          for (var i = 0; i < reader.FieldCount; i++)
          {
            dic[reader.GetName(i)] = this.castDbValue<T>(reader.GetValue(i));
          }

          break;
        }
      };

      if (command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          action();
        }
      }
      else
      {
        action();
      }

      return dic;
    }

    private T castDbValue<T>(object value)
    {
      if (typeof(T) == typeof(string))
      {
        return (T)(object)value.ToString();
      }
      else
      {
        return (T)value;
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
  }
}
