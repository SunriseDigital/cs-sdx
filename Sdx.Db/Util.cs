using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.SqlClient;
using System.Data;

namespace Sdx.Db
{
  public class Util
  {
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
  }
}
