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
      private class Condition
      {
        public Where Where { get; set; }
        public string Table { get; set; }
        public Comparison Comparison { get; set; }
        public Logical Logical { get; set; }
        public string Column { get; set; }
        public object Value { get; set; }
      }

      private class ConditionCount
      {
        private int value = 0;
        public int Value { get { return this.value; } }
        public void Incr()
        {
          ++this.value;
        }
      }

      private List<Condition> wheres = new List<Condition>();
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
        this.Add(new Condition
        {
          Where = where,
          Logical = logical
        });
        return this;
      }

      public Where Add(String column, Object value, String table = null, Comparison comparison = Comparison.Equal, Logical logical = Logical.And)
      {
        if (table == null)
        {
          table = this.Table;
        }

        this.Add(new Condition
        {
          Column = column,
          Table = table,
          Logical = logical,
          Comparison = comparison,
          Value = value
        });

        return this;
      }

      private void Add(Condition cond)
      {
        if(cond.Logical == Logical.Or && this.wheres.Count == 0)
        {
          //一番最初のWhereがOrで足されたということは何かプログラマーの意図と違うことが起こってるはず。
          throw new Exception("Illegal logical operation for the first where condition `"+cond.Logical.SqlString()+"`");
        }

        this.wheres.Add(cond);
      }

      private void Build(DbCommand command, ConditionCount condCount)
      {
        string whereString = "";

        wheres.ForEach(cond =>
        {
          if (whereString.Length > 0)
          {
            whereString += cond.Logical.SqlString();
          }

          if (cond.Where != null)
          {
            if(condCount.Value > 0)
            {
              command.CommandText += cond.Logical.SqlString();
            }
            cond.Where.Build(command, condCount);
          }
          else
          {
            var placeHolder = "@" + cond.Column + "@{0}@" + condCount.Value;
            if (cond.Table != null)
            {
              placeHolder = String.Format(placeHolder, cond.Table);
              whereString += String.Format(
                "{0}.{1} = {2}",
                this.factory.QuoteIdentifier(cond.Table),
                this.factory.QuoteIdentifier(cond.Column),
                placeHolder
              );
            }
            else
            {
              placeHolder =  String.Format(placeHolder, "_");
              whereString += String.Format(
                "{0} = {1}",
                this.factory.QuoteIdentifier(cond.Column),
                placeHolder
              );
            }

            command.Parameters.Add(this.factory.CreateParameter(placeHolder, cond.Value.ToString()));
            condCount.Incr();
          }
        });

        if (this.EnableBracket)
        {
          whereString = "(" + whereString + ")";
        }

        command.CommandText += whereString;
      }

      public void Build(DbCommand command)
      {
        this.Build(command, new ConditionCount());
      }

      public DbCommand Build()
      {
        var command = this.factory.CreateCommand();
        this.Build(command);
        return command;
      }
    }
}
