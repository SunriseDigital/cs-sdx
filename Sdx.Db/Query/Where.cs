using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;

namespace Sdx.Db.Query
{
    public enum Logical
    {
      And,
      Or
    }

    public enum Comparison
    {
      Equal,
      NotEqual,
      AltNotEqual,
      GreaterThan,
      Less_than,
      GreaterEqual,
      LessEqual,
      Like,
      NotLike,
      In,
      NotIn
    }

    public static class WhereEnumExtension
    {
      public static string SqlString(this Logical logical)
      {
        string[] strings = { " AND ", " OR "};
        return strings[(int)logical];
      }

      public static string SqlString(this Comparison comp)
      {
        string[] strings = {
          " = ",
          " <> ",
          " != ",
          " > ",
          " < ",
          " >= ",
          " <= ",
          " LIKE ",
          " NOT LIKE ",
          " IN ",
          " NOT IN "
        };
        return strings[(int)comp];
      }
    }

    public class Where
    {
      private List<Dictionary<string, object>> wheres = new List<Dictionary<string, object>>();
      private Factory factory;

      public int Count
      {
        get { return wheres.Count; }
      }

      public bool EnableBracket { get; set; }

      public string Table { get; set; }

      public Where(Factory factory)
      {
        this.factory = factory;
      }

      public Where Add(Where where, Logical logical = Logical.And)
      {
        where.EnableBracket = true;
        this.wheres.Add(new Dictionary<String, Object> {
          {"where", where},
          {"logical", logical}
        });
        return this;
      }

      public Where Add(String column, Object value, String table = null, Comparison comparison = Comparison.Equal, Logical logical = Logical.And)
      {
        if (table == null)
        {
          table = this.Table;
        }

        this.wheres.Add(new Dictionary<String, Object> {
          {"column", column},
          {"table", table},
          {"value", value},
          {"comparison", comparison},
          {"logical", logical}
        });
        return this;
      }

      public int Build(DbCommand command, int startIndex = 0)
      {
        string whereString = "";
        if (this.EnableBracket)
        {
          whereString = "(";
        }

        int loopCount = 0;
        wheres.ForEach(dic =>
        {
          if (loopCount > 0)
          {
            whereString += ((Logical)dic["logical"]).SqlString();
          }

          loopCount++;

          if (dic.ContainsKey("table"))
          {
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
            ++startIndex;
          }
          else if (dic.ContainsKey("where"))
          {
            Where where = dic["where"] as Where;
            startIndex = where.Build(command, startIndex);
          }

          
        });

        if (this.EnableBracket)
        {
          whereString += ")";
        }

        command.CommandText += whereString;
        return startIndex;
      }

      public DbCommand Build()
      {
        var command = this.factory.CreateCommand();
        this.Build(command);
        return command;
      }
    }
}
