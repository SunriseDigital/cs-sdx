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

    public ResultSet Group(string contextName)
    {
      var resultSet = new ResultSet();
      resultSet.Build(this.list, this.select, contextName);
      

      return resultSet;
    }

    internal void AddRow(Dictionary<string, object> row)
    {
      this.list.Add(row);
    }
  }
}
