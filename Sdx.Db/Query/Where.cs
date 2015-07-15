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

      public string Table { get; set; }

      public Where(Factory factory)
      {
        this.factory = factory;
      }

      public Where Add(String column, Object value, String table = null)
      {

        Console.WriteLine(String.Format("{0}/{1}/{2}", column, table, this.Table));
        if (table == null)
        {
          table = this.Table;
        }

        wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value}
        });
        return this;
      }

      public void Build(DbCommand command, int startIndex = 0)
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

            var placeHolder = "@" + dic["column"] + "@{0}@" + startIndex.ToString();
            if (dic["table"] != null)
            {
              placeHolder = String.Format(placeHolder, dic["table"]);
              whereString += String.Format(
                "{0}.{1} = {2}",
                this.factory.QuoteIdentifier(dic["table"] as String),
                this.factory.QuoteIdentifier(dic["column"] as String),
                placeHolder
              );
            }
            else
            {
              placeHolder =  String.Format(placeHolder, "_");
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

          ++startIndex;
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
