using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sdx.Db.Query;

namespace Sdx.Db
{
  public class Record
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


    //public Record Filter<T>(string contextName) where T : Record, new()
    //{
    //  Record newResult = new T();
    //  Console.WriteLine(this.select.Context(contextName));
      
    //  return newResult;
    //}

    public RecordSet<T> Group<T>(string contextName) where T : Record, new()
    {
      var resultSet = new RecordSet<T>();
      resultSet.Build(this.list, this.select, contextName);
      

      return resultSet;
    }

    internal void AddRow(Dictionary<string, object> row)
    {
      this.list.Add(row);
    }

    //public ResultSet GetResultSet(string contextName)
    //{
    //  if (this.select != null)
    //  {
    //    if (this.select.HasContext(contextName))
    //    {
    //      return this.Group(contextName);
    //    }
    //  }

      

    //}
  }
}
