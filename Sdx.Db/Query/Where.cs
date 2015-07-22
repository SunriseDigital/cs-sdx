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

      internal class ConditionCount
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

      public Where AddOr(Where where)
      {
        this.Add(where, Logical.Or);
        return this;
      }

      public Where AddOr(String column, Object value, String table = null, Comparison comparison = Comparison.Equal)
      {
        this.Add(column, value, Logical.Or, table, comparison);
        return this;
      }

      public Where Add(Where where)
      {
        this.Add(where, Logical.And);
        return this;
      }

      public Where Add(String column, Object value, String table = null, Comparison comparison = Comparison.Equal)
      {
        this.Add(column, value, Logical.And, table, comparison);
        return this;
      }

      private Where Add(Where where, Logical logical)
      {
        where.EnableBracket = true;
        this.Add(new Condition
        {
          Where = where,
          Logical = logical
        });
        return this;
      }

      private Where Add(String column, Object value, Logical logical, String table = null, Comparison comparison = Comparison.Equal)
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

      internal string Build(DbParameterCollection parameters, ConditionCount condCount)
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
            whereString += cond.Where.Build(parameters, condCount);
          }
          else
          {
            whereString += this.BuildValueConditionString(parameters, cond, condCount);
            condCount.Incr();
          }
        });

        if (this.EnableBracket)
        {
          whereString = "(" + whereString + ")";
        }

        return whereString;
      }

      private string BuildValueConditionString(DbParameterCollection parameters, Condition cond, ConditionCount condCount)
      {
        string rightHand;
        if (cond.Value is Expr)
        {
          rightHand = cond.Value.ToString();
        }
        else if (cond.Value is Select)
        {
          Select select = (Select)cond.Value;
          rightHand = "(" + select.BuildSelectString(parameters, condCount) + ")";
        }
        else
        {
          rightHand = "@" + cond.Column + "@" + condCount.Value;
          parameters.Add(this.factory.CreateParameter(rightHand, cond.Value.ToString()));
        }
        
        if (cond.Table != null)
        {
          return String.Format(
            "{0}.{1}{2}{3}",
            this.factory.QuoteIdentifier(cond.Table),
            this.factory.QuoteIdentifier(cond.Column),
            cond.Comparison.SqlString(),
            rightHand
          );
        }
        else
        {
          return String.Format(
            "{0}{1}{2}",
            this.factory.QuoteIdentifier(cond.Column),
            cond.Comparison.SqlString(),
            rightHand
          );
        }
      }

      public void Build(DbCommand command)
      {
        command.CommandText += this.Build(command.Parameters, new ConditionCount());
      }

      public DbCommand Build()
      {
        var command = this.factory.CreateCommand();
        this.Build(command);
        return command;
      }
    }
}
