using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Scaffold.Group
{
  public class TableMeta : Base
  {
    private Db.TableMeta tableMeta;
    private Config.Value display;
    private Config.Value methodForList;

    public TableMeta(string columnName, Db.TableMeta tableMeta, Config.Value display, Config.Value methodForList = null)
      : base(columnName, methodForList != null)
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
        if(display.IsString)
        {
          name = record.GetString((string)display);
        }
        else
        {
          name = (string)display.Invoke(record.GetType(), record, null);
        }
        
      }

      return name;
    }

    protected internal override List<KeyValuePair<string, string>> BuildPairListForSelector(Sdx.Db.Connection conn)
    {
      if(!HasSelector)
      {
        return null;
      }

      var table = tableMeta.CreateTable();
      return (List<KeyValuePair<string, string>>)methodForList.Invoke(table.GetType(), table, new object[] { conn }); ;
    }
  }
}
