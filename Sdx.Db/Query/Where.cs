using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
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
      private Select select;

      internal int Count
      {
        get { return wheres.Count; }
      }

      internal bool EnableBracket { get; set; }

      internal Table Table { get; set; }

      public Where(Select select)
      {
        this.select = select;
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

      internal string Build(DbParameterCollection parameters, Counter condCount)
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
            parameters.Add(this.select.Adapter.CreateParameter(holder, value.ToString()));
            inCond += holder;
            condCount.Incr();
          }

          rightHand = "(" + inCond + ")";
        }
        else
        {
          rightHand = "@" + condCount.Value;
          parameters.Add(this.select.Adapter.CreateParameter(rightHand, cond.Value.ToString()));
          condCount.Incr();
        }

        var leftHand = cond.Column.Build(this.select.Adapter);

        this.select.TableList.ForEach(table => {
          leftHand = leftHand.Replace("{"+table.ContextName+"}", this.select.Adapter.QuoteIdentifier(table.ContextName));
        });

        return String.Format(
          "{0}{1}{2}",
          leftHand,
          cond.Comparison.SqlString(),
          rightHand
        );
      }
    }
}
