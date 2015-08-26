using System;
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
    public Query.Profiler Profiler { get; set; }

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
      if (this.Profiler != null)
      {
        query = this.Profiler.Begin(command);
      }

      var reader = command.ExecuteReader();

      if (query != null)
      {
        query.End();
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
        throw new Exception("QuoteIdentifier support only Query.Expr or string, "+obj.GetType()+" given.");
      }
    }

    internal abstract string AppendLimitQuery(string selectSql, int limit, int offset);

    public T FetchRecord<T>(Query.Select select) where T : Record, new()
    {
      var resultSet = this.FetchRecordSet<T>(select);

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
    public RecordSet<T> FetchRecordSet<T>(Query.Select select) where T : Record, new()
    {
      select.Adapter = this;

      var prop = typeof(T).GetProperty("Meta");
      if (prop == null)
      {
        throw new NotImplementedException("Missing Meta property in " + typeof(T));
      }

      var meta = prop.GetValue(null, null) as MetaData;
      if (meta == null)
      {
        throw new NotImplementedException("Initialize TableMeta for " + typeof(T));
      }

      var contextName = meta.Name;

      var command = select.Build();
      var resultSet = new RecordSet<T>();
      using (var con = this.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = this.ExecuteReader(command);
        resultSet.Build(reader, select, contextName);
      }

      return resultSet;
    }

    public List<Dictionary<string, object>> FetchDictionaryList(Query.Select select)
    {
      select.Adapter = this;
      return this.FetchDictionaryList(select.Build());
    }

    public List<Dictionary<string, object>> FetchDictionaryList(DbCommand command)
    {
      var list = new List<Dictionary<string, object>>();

      using (var con = this.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          var row = new Dictionary<string, object>();
          for (var i = 0; i < reader.FieldCount; i++)
          {
            row[reader.GetName(i)] = reader.GetValue(i);
          }

          list.Add(row);
        }
      }

      return list;
    }

    public List<KeyValuePair<object, object>> FetchKeyValuePairList(Query.Select select)
    {
      select.Adapter = this;
      return this.FetchKeyValuePairList(select.Build());
    }

    public List<KeyValuePair<object, object>> FetchKeyValuePairList(DbCommand command)
    {
      var list = new List<KeyValuePair<object, object>>();

      using (var con = this.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          var row = new KeyValuePair<object, object>(
            reader.GetValue(0),
            reader.GetValue(1)
          );

          list.Add(row);
        }
      }

      return list;
    }

    public List<T> FetchList<T>(Query.Select select)
    {
      select.Adapter = this;
      return this.FetchList<T>(select.Build());
    }

    public List<T> FetchList<T>(DbCommand command)
    {
      var list = new List<T>();
      using (var con = this.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = this.ExecuteReader(command);
        while (reader.Read())
        {
          list.Add(this.castDbValue<T>(reader.GetValue(0)));
        }
      }

      return list;
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
  }
}
