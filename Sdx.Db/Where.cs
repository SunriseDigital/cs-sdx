using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sdx.Db
{
    public class Where
    {
      private List<Object> wheres = new List<object>();

      private static string defaultProviderName = "System.Data.SqlClient";

      private string providerName;

      public Where add(String column, Object value, String table = null)
      {
        wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public static string DefaultProviderName
      {
        get
        {
          return defaultProviderName;
        }
        set
        {
          defaultProviderName = value;
        }
      }

      public string ProviderName
      {
        get
        {
          if(this.providerName != null)
          {
            return this.providerName;
          }

          return DefaultProviderName;
        }
        set
        {
          this.providerName = value;
        }
      }

      public SqlCommand build()
      {
        var command = new SqlCommand();
        DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);
        DbCommandBuilder builder = factory.CreateCommandBuilder();
        wheres.ForEach(obj => {
          if(command.CommandText.Length > 0)
          {
            command.CommandText += " AND ";
          }

          if(obj is Dictionary<String, Object>)
          {
            Dictionary<String, Object> dic = obj as Dictionary<String, Object>;
            String placeHolder = "@" + dic["table"] + "@" + dic["column"]; ;
            if (dic["table"] != null)
            {
              command.CommandText += String.Format(
                "{0}.{1} = {2}",
                builder.QuoteIdentifier(dic["table"] as String),
                builder.QuoteIdentifier(dic["column"] as String),
                placeHolder
              );
            }
            else
            {
              command.CommandText += String.Format(
                "{0} = {1}",
                builder.QuoteIdentifier(dic["column"] as String),
                placeHolder
              );
            }


            command.Parameters.AddWithValue(placeHolder, dic["value"].ToString());
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
