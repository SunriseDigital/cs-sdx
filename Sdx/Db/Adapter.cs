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

    internal abstract void InitSelectEvent(Select select);

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
        //Select.SetCommentのため、最後がコメントかチェックする
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

    internal T Fetch<T>(DbCommand command, Func<T> func)
    {
      T result = default(T);
      if (command.Connection == null)
      {
        using (var con = this.CreateConnection())
        {
          con.Open();
          command.Connection = con;
          result = func();
        }
      }
      else
      {
        result = func();
      }

      return result;
    }

    public List<Dictionary<string, T>> FetchDictionaryList<T>(DbCommand command)
    {
      return this.Fetch<List<Dictionary<string, T>>>(command, () => {
        var list = new List<Dictionary<string, T>>();

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

        return list;
      });
    }

    public List<KeyValuePair<TKey, TValue>> FetchKeyValuePairList<TKey, TValue>(DbCommand command)
    {
      return this.Fetch<List<KeyValuePair<TKey, TValue>>>(command, () => {
        var list = new List<KeyValuePair<TKey, TValue>>();
        var reader = this.ExecuteReader(command);
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

    public List<T> FetchList<T>(DbCommand command)
    {
      return this.Fetch<List<T>>(command, () => {
        var list = new List<T>();
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          list.Add(this.castDbValue<T>(reader.GetValue(0)));
        }
        return list;
      });
    }

    public T FetchOne<T>(DbCommand command)
    {
      return this.Fetch<T>(command, () => {
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          return this.castDbValue<T>(reader.GetValue(0));
        }
        return default(T);
      });
    }

    public Dictionary<string, T> FetchDictionary<T>(DbCommand command)
    {
      return this.Fetch<Dictionary<string, T>>(command, () => {
        var dic = new Dictionary<string, T>();
        var reader = this.ExecuteReader(command);
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
