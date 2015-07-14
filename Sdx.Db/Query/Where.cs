using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sdx.Db.Query
{
    public class Where
    {
      private List<Object> wheres = new List<object>();
      private Factory factory;

      public Where(Factory factory)
      {
        this.factory = factory;
      }

      public Where add(String column, Object value, String table = null)
      {
        wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public DbCommand build()
      {
        var command = this.factory.CreateCommand();
        var builder = this.factory.CreateCommandBuilder();
        
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

            command.Parameters.Add(this.factory.CreateParameter(placeHolder, dic["value"].ToString()));
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
