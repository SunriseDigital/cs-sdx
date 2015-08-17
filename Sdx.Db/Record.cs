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

    public RecordSet<T> GetRecordSet<T>(string contextName, Action<Select> selectHook = null) where T : Record, new()
    {
      if (selectHook == null && this.select.HasContext(contextName))
      {
        var resultSet = new RecordSet<T>();
        resultSet.Build(this.list, this.select, contextName);
        return resultSet;
      }
      else
      {
        var table = this.select.Context(this.ContextName).Table;
        if (table.TableMeta.Relations.ContainsKey(contextName))
        {
          var relations = table.TableMeta.Relations[contextName];
          var db = this.select.Adapter;
          var select = db.CreateSelect();
          select.AddFrom(relations.Table)
            .Where.Add(relations.ReferenceKey, this.GetString(relations.ForeignKey));

          if (selectHook != null)
          {
            selectHook.Invoke(select);
          }

          return select.Execute<T>(contextName);
        }

        throw new Exception("Missing relation setting for " + contextName);
      }
    }

    internal void AddRow(Dictionary<string, object> row)
    {
      this.list.Add(row);
    }
  }
}
