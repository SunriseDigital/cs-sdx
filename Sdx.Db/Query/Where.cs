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

      public int Count
      {
        get { return wheres.Count; }
      }

      public Where(Factory factory)
      {
        this.factory = factory;
      }

      public Where Add(String column, Object value, String table = null)
      {
        wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public void Build(DbCommand command)
      {
        string whereString = "";
        wheres.ForEach(obj =>
        {
          if (whereString.Length > 0)
          {
            whereString += " AND ";
          }

          if (obj is Dictionary<String, Object>)
          {
            Dictionary<String, Object> dic = obj as Dictionary<String, Object>;
            String placeHolder;
            if (dic["table"] != null)
            {
              placeHolder = "@" + dic["table"] + "@" + dic["column"];
              whereString += String.Format(
                "{0}.{1} = {2}",
                this.factory.QuoteIdentifier(dic["table"] as String),
                this.factory.QuoteIdentifier(dic["column"] as String),
                placeHolder
              );
            }
            else
            {
              placeHolder = "@" + dic["column"];
              whereString += String.Format(
                "{0} = {1}",
                this.factory.QuoteIdentifier(dic["column"] as String),
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

        command.CommandText += whereString;
      }

      public DbCommand Build()
      {
        var command = this.factory.CreateCommand();
        this.Build(command);
        return command;
      }
    }
}
