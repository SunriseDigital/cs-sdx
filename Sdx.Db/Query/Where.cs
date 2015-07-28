using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;

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
        public Comparison Comparison { get; set; }
        public Logical Logical { get; set; }
        public Column Column { get; set; }
        public object Value { get; set; }
      }

      private List<Condition> wheres = new List<Condition>();
      private Factory factory;

      public int Count
      {
        get { return wheres.Count; }
      }

      public bool EnableBracket { get; set; }

      public Table Table { get; set; }

      public Where(Factory factory)
      {
        this.factory = factory;
      }

      public Where AddOr(Where where)
      {
        this.AddWhere(where, Logical.Or);
        return this;
      }

      public Where AddOr(Object column, Object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.Or, comparison);
        return this;
      }

      public Where Add(Where where)
      {
        this.AddWhere(where, Logical.And);
        return this;
      }

      public Where Add(Object column, Object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.And, comparison);
        return this;
      }

      private Where AddWhere(Where where, Logical logical)
      {
        where.EnableBracket = true;
        this.Add(new Condition
        {
          Value = where,
          Logical = logical
        });
        return this;
      }

      private Where AddColumn(Object columnName, Object value, Logical logical, Comparison comparison)
      {
        var column = new Column(columnName);
        column.Table = this.Table;

        this.Add(new Condition
        {
          Column = column,
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

      public string Build(DbParameterCollection parameters, Counter condCount)
      {
        string whereString = "";

        wheres.ForEach(cond =>
        {
          if (whereString.Length > 0)
          {
            whereString += cond.Logical.SqlString();
          }

          if (cond.Value is Where)
          {
            Where where = cond.Value as Where;
            whereString += where.Build(parameters, condCount);
          }
          else
          {
            whereString += this.BuildValueConditionString(parameters, cond, condCount);
          }
        });

        if (this.EnableBracket)
        {
          whereString = "(" + whereString + ")";
        }

        return whereString;
      }

      private string BuildValueConditionString(DbParameterCollection parameters, Condition cond, Counter condCount)
      {
        string rightHand;
        if (cond.Value is Expr)
        {
          rightHand = cond.Value.ToString();
          condCount.Incr();
        }
        else if (cond.Value is Select)
        {
          Select select = (Select)cond.Value;
          rightHand = "(" + select.BuildSelectString(parameters, condCount) + ")";
        }
        //IEnumerable<>かどうかチェック。
        else if (!(cond.Value is string) && cond.Value.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        {
          cond.Comparison = Comparison.In;
          string inCond = "";
          var list = cond.Value as IEnumerable<object>;
          foreach(var value in list)
          {
            if(inCond != "")
            {
              inCond += ", ";
            }
            string holder = "@" + condCount.Value;
            parameters.Add(this.factory.CreateParameter(holder, value.ToString()));
            inCond += holder;
            condCount.Incr();
          }

          rightHand = "(" + inCond + ")";
        }
        else
        {
          rightHand = "@" + condCount.Value;
          parameters.Add(this.factory.CreateParameter(rightHand, cond.Value.ToString()));
          condCount.Incr();
        }

        return String.Format(
          "{0}{1}{2}",
          cond.Column.Build(this.factory),
          cond.Comparison.SqlString(),
          rightHand
        );
      }
    }
}
