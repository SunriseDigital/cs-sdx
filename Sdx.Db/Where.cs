using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sdx.Db
{
    public class Where
    {
      private List<Object> wheres = new List<object>();

      private static DbCommandBuilder defaultCommandBuilder;

      private DbCommandBuilder commandBuilder;

      public Where add(String column, Object value, String table = null)
      {
        wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public static DbCommandBuilder DefaultCommandBuilder
      {
        get
        {
          if(defaultCommandBuilder == null)
          {
            DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            defaultCommandBuilder = factory.CreateCommandBuilder();
          }

          return defaultCommandBuilder;
        }
        set
        {
          defaultCommandBuilder = value;
        }
      }

      public DbCommandBuilder CommandBuilder
      {
        get
        {
          if(this.commandBuilder != null)
          {
            return this.commandBuilder;
          }

          return DefaultCommandBuilder;
        }
        set
        {
          this.commandBuilder = value;
        }
      }

      public SqlCommand build()
      {
        var command = new SqlCommand();
        
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
                this.CommandBuilder.QuoteIdentifier(dic["table"] as String),
                this.CommandBuilder.QuoteIdentifier(dic["column"] as String),
                placeHolder
              );
            }
            else
            {
              command.CommandText += String.Format(
                "{0} = {1}",
                this.CommandBuilder.QuoteIdentifier(dic["column"] as String),
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
