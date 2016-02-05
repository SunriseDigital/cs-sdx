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

    public TableMeta(string columnName, Db.TableMeta tableMeta, string display, string methodForList):base(columnName)
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
        var table = tableMeta.CreateTable<Db.Table>();
        var method = table.GetType().GetMethod("FindByPkey").MakeGenericMethod(tableMeta.RecordType);
        var record = (Db.Record)method.Invoke(table, new object[] { TargetValue, conn });
        Sdx.Context.Current.Debug.Log(record);
        name = record.GetString(display);
      }

      return name;
    }
  }
}
