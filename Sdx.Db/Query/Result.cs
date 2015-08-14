using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Sdx.Db.Query
{
  public class Result
  {
    private Select select;

    private List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

    public Result()
    {

    }

    internal Result(Select select):base()
    {
      this.select = select;
      var command = this.select.Build();
      using (var con = this.select.Adapter.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = command.ExecuteReader();
        var schemaTable = reader.GetSchemaTable();
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
    }

    internal string ContextName { get; set; }

    internal Select Select
    {
      get
      {
        return this.select;
      }
      set
      {
        this.select = value;
      }
    }

    public string GetString(string key)
    {
      // TODO Implements for missing ContextName.

      key = key + "@" + this.ContextName;
      if (this.list[0].ContainsKey(key))
      {
        return this.list[0][key].ToString();
      }

      return "";
    }


    public Result Filter<T>(string contextName) where T : Result, new()
    {
      Result newResult = new T();
      Console.WriteLine(this.select.Context(contextName));
      
      return newResult;
    }

    public List<Result> Assemble(string contextName)
    {
      Table table = this.select.Context(contextName).Table;

      var dupCheckDic = new Dictionary<List<object>, Result>();
      var resultList = new List<Result>();
      this.list.ForEach(row => {
        var pkeys = table.Meta.Pkeys;
        if (pkeys == null)
        {
          throw new Exception("Missing Pkeys setting in " + table.ToString() + ".Meta");
        }

        var key = this.buildPkeyValues(row, pkeys, contextName);
        Result result;
        if (!dupCheckDic.ContainsKey(key))
        {
          result = new Result();
          result.ContextName = contextName;
          result.Select = this.select;
        }
        else
        {
          result = dupCheckDic[key];
        }

        result.AddRow(row);

        resultList.Add(result);

      });

      return resultList;
    }

    private void AddRow(Dictionary<string, object> row)
    {
      this.list.Add(row);
    }

    private List<object> buildPkeyValues(Dictionary<string, object> row, List<string> pkeys, string contextName)
    {
      var values = new List<object>();

      pkeys.ForEach(column => {
        values.Add(row[column + "@" + contextName]);
      });

      return values;
    }
  }
}
