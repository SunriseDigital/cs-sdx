using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Common;
using System.Data;
using System.Data.SqlClient;

namespace Sdx.Db
{
  public static class Util
  {
    /// <summary>
    /// デバッグやロギング用です。サニタイズしませんので注意してください。
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static String CommandToSql(DbCommand command)
    {
      DbType[] quotedParameterTypes = new DbType[] {
          DbType.AnsiString, DbType.Date,
          DbType.DateTime, DbType.Guid, DbType.String,
          DbType.AnsiStringFixedLength, DbType.StringFixedLength
      };
      string query = command.CommandText;

      var clonedParams = new DbParameter[command.Parameters.Count];
      command.Parameters.CopyTo(clonedParams, 0);

      foreach (DbParameter param in clonedParams.OrderByDescending(p => p.ParameterName.Length))
      {
        string value = param.Value.ToString();
        if (quotedParameterTypes.Contains(param.DbType))
        {
          value = "'" + value + "'";
        }
          
        query = query.Replace(param.ParameterName, value);
      }

      return query;
    }

    /// <summary>
    /// カラム名が同じだと上書きされるので注意してください。
    /// asで別名を付ければ取得可能です。
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static List<Dictionary<string, string>> CreateDictinaryList(DbDataReader reader)
    {
      DataTable schemaTable = reader.GetSchemaTable();

      List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
      foreach (IDataRecord record in reader)
      {
        int key = 0;
        Dictionary<string, string> keyValue = new Dictionary<string, string>();
        list.Add(keyValue);

        //{カラム名 => 値}のDictionaryを作成
        //同じカラムネームだと上書きされるので注意。
        foreach (DataRow row in schemaTable.Rows)
        {
          keyValue[row["ColumnName"].ToString()] = record.GetValue(key++).ToString();
        }
      }

      return list;
    }

    public static Dictionary<string, object> ToDictionary(DataRow row)
    {
      Dictionary<string, object> dic = new Dictionary<string, object>();
      foreach (DataColumn column in row.Table.Columns)
      {
        dic[column.ToString()] = row[column.ToString()];
      }

      return dic;
    }
  }
}
