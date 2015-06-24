﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using System.Data;

namespace Sdx.Db
{
  public class Util
  {
    /// <summary>
    /// デバッグやロギング用です。サニタイズしませんので注意してください。
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public static String SqlCommandToSql(SqlCommand command)
    {
      DbType[] quotedParameterTypes = new DbType[] {
          DbType.AnsiString, DbType.Date,
          DbType.DateTime, DbType.Guid, DbType.String,
          DbType.AnsiStringFixedLength, DbType.StringFixedLength
      };
      string query = command.CommandText;

      var clonedParams = new SqlParameter[command.Parameters.Count];
      command.Parameters.CopyTo(clonedParams, 0);


      foreach (SqlParameter param in clonedParams.OrderByDescending(p => p.ParameterName.Length))
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
    public static List<Dictionary<string, string>> CreateDictinaryList(SqlDataReader reader)
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
  }
}
