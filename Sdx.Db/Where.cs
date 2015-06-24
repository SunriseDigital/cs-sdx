using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sdx.Db
{
    public class Where
    {
      private List<Object> wheres = new List<object>();

      public Where add(String column, String value, String table = null)
      {
        wheres.Add(new Dictionary<String, String> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public SqlCommand build()
      {
        var result = this.Connection.CreateCommand();
        DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        DbCommandBuilder builder = factory.CreateCommandBuilder();
        wheres.ForEach(obj => {
          if(obj is Dictionary<String, String>)
          {
            Dictionary<String, String> dic = obj as Dictionary<String, String>;
            result.CommandText = String.Format(
              "{0} = {1}",
              builder.QuoteIdentifier(dic["column"]),
              "@"+dic["column"]
            );

            result.Parameters.AddWithValue("@" + dic["column"], dic["value"]);
          }
          else if (obj is Where)
          {
            Where where = obj as Where;
          }
        });

        return result;
      }

      public SqlConnection Connection { get; set; }
    }
}
