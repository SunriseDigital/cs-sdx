using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using Sdx.Db.Query;

namespace Sdx.Db
{
  public abstract class Adapter
  {
    internal DbProviderFactory Factory { get; private set; }
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
      this.Factory = this.GetFactory();
      this.builder = this.Factory.CreateCommandBuilder();
    }

    public Connection CreateConnection()
    {
      var con = new Connection(this);
      con.ConnectionString = this.ConnectionString;
      return con;
    }

    public DbParameter CreateParameter(string key, string value)
    {
      DbParameter param = this.Factory.CreateParameter();
      param.ParameterName = key;
      param.Value = value;

      return param;
    }

    internal abstract void InitSelectEvent(Select select);

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

    private T ExecuteCommand<T>(DbCommand command, string comment, Func<T> func)
    {
      Query.Log query = null;
      if (Sdx.Context.Current.DbProfiler != null)
      {
        query = Sdx.Context.Current.DbProfiler.Begin(command);
      }

      T result = default(T);
      try
      {
        result = func();
      }
      catch (Exception e)
      {
        throw new Sdx.Db.DbException(e.Message + "\n" + command.CommandText);
      }

      if (query != null)
      {
        query.End();
        query.Adapter = this;
        query.Comment = comment;
      }

      return result;
    }

    public object ExecuteScalar(DbCommand command)
    {
      return this.ExecuteCommand<object>(command, null, () => {
        return command.ExecuteScalar();
      });
    }

    public int ExecuteNonQuery(DbCommand command)
    {
      return this.ExecuteCommand<int>(command, null, () => {
        return command.ExecuteNonQuery();
      });
    }

    public DbDataReader ExecuteReader(DbCommand command)
    {
      string comment = null;
      if(command.Parameters.Count > 0)
      {
        //Select.SetCommentのため、最後がコメントかチェックする
        var lastIndex = command.Parameters.Count - 1;
        var param = command.Parameters[lastIndex];
        if (param.ParameterName == Query.Select.CommentParameterKey)
        {
          command.Parameters.RemoveAt(lastIndex);
          comment = param.Value.ToString();
        }
      }

      return this.ExecuteCommand<DbDataReader>(command, comment, () => {
        return command.ExecuteReader();
      });
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

    internal T Fetch<T>(DbCommand command, Connection connection, Func<DbDataReader, T> func)
    {
      if (connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con.DbConnection;
          using (var reader = this.ExecuteReader(command))
          {
            return func(reader);
          }
        }
      }
      else
      {
        command.Connection = connection.DbConnection;
        command.Transaction = connection.DbTransaction;
        using (var reader = this.ExecuteReader(command))
        {
          return func(reader);
        }
      }
    }

    internal abstract ulong FetchLastInsertId(Connection connection);

    public List<Dictionary<string, T>> FetchDictionaryList<T>(DbCommand command, Connection connection = null)
    {
      return this.Fetch<List<Dictionary<string, T>>>(command, connection, (reader) => {
        var list = new List<Dictionary<string, T>>();

        while (reader.Read())
        {
          var row = new Dictionary<string, T>();
          for (var i = 0; i < reader.FieldCount; i++)
          {
            row[reader.GetName(i)] = this.castDbValue<T>(reader.GetValue(i));
          }

          list.Add(row);
        }

        return list;
      });
    }

    public List<KeyValuePair<TKey, TValue>> FetchKeyValuePairList<TKey, TValue>(DbCommand command, Connection connection = null)
    {
      return this.Fetch<List<KeyValuePair<TKey, TValue>>>(command, connection, (reader) => {
        var list = new List<KeyValuePair<TKey, TValue>>();
        while (reader.Read())
        {
          var row = new KeyValuePair<TKey, TValue>(
            this.castDbValue<TKey>(reader.GetValue(0)),
            this.castDbValue<TValue>(reader.GetValue(1))
          );

          list.Add(row);
        }
        return list;
      });
    }

    public List<T> FetchList<T>(DbCommand command, Connection connection = null)
    {
      return this.Fetch<List<T>>(command, connection, (reader) => {
        var list = new List<T>();
        while (reader.Read())
        {
          list.Add(this.castDbValue<T>(reader.GetValue(0)));
        }
        return list;
      });
    }

    public T FetchOne<T>(DbCommand command, Connection connection = null)
    {
      return this.Fetch<T>(command, connection, (reader) => {
        while (reader.Read())
        {
          return this.castDbValue<T>(reader.GetValue(0));
        }

        return default(T);
      });
    }

    public Dictionary<string, T> FetchDictionary<T>(DbCommand command, Connection connection = null)
    {
      return this.Fetch<Dictionary<string, T>>(command, connection, (reader) => {
        var dic = new Dictionary<string, T>();
        while (reader.Read())
        {
          for (var i = 0; i < reader.FieldCount; i++)
          {
            dic[reader.GetName(i)] = this.castDbValue<T>(reader.GetValue(i));
          }

          break;
        }

        return dic;
      });
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

    public Query.Select CreateSelect()
    {
      return new Query.Select(this);
    }
  }
}
