using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sdx.Db
{
    public class Where
    {
      private List<Object> wheres = new List<object>();

      public Where add(String column, Object value, String table = null)
      {
        wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public SqlCommand build()
      {
        var command = new SqlCommand();
        DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        DbCommandBuilder builder = factory.CreateCommandBuilder();
        wheres.ForEach(obj => {
          if(command.CommandText.Length > 0)
          {
            command.CommandText += " AND ";
          }

          if(obj is Dictionary<String, Object>)
          {
            Dictionary<String, Object> dic = obj as Dictionary<String, Object>;
            command.CommandText += String.Format(
              "{0} = {1}",
              builder.QuoteIdentifier(dic["column"] as String),
              "@"+dic["column"]
            );

            command.Parameters.AddWithValue("@" + dic["column"], dic["value"].ToString());
          }
          else if (obj is Where)
          {
            Where where = obj as Where;
          }
        });

        return command;
      }
    }
}
