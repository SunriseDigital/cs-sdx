using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public class TableMeta : Base
  {
    private Db.TableMeta tableMeta;
    private string display;
    private string methodForList;

    public TableMeta(string columnName, Db.TableMeta tableMeta, string display, string methodForList = null):base(columnName, methodForList != null)
    {
      this.tableMeta = tableMeta;
      this.display = display;
      this.methodForList = methodForList;
    }

    protected override string FetchName()
    {
      var name = "";
      using(var conn = Manager.Db.CreateConnection())
      {
        conn.Open();
        var table = tableMeta.CreateTable();
        var record = conn.FetchRecordByPkey(table, TargetValue);
        name = record.GetString(display);
      }

      return name;
    }

    protected internal override List<KeyValuePair<string, string>> GetKeyValuePairList()
    {
      List<KeyValuePair<string, string>> result = null;
      using (var conn = Manager.Db.CreateConnection())
      {
        conn.Open();
        var table = tableMeta.CreateTable();
        var select = Manager.Db.CreateSelect();
        select.AddFrom(table);
        var method = table.GetType().GetMethod(this.methodForList);
        if (method == null)
        {
          throw new InvalidOperationException("Missing Select Method " + this.methodForList + " in " + table);
        }

        method.Invoke(table, new object[] { select });
        result = conn.FetchKeyValuePairList<string, string>(select);
      }

      return result;
    }
  }
}
