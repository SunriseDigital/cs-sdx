using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
    public class Condition
    {
      private class Holder
      {
        public Comparison Comparison { get; set; }
        public Logical Logical { get; set; }
        public Column Column { get; set; }
        public object Value { get; set; }
      }

      private List<Holder> wheres = new List<Holder>();
      private Select select;

      internal int Count
      {
        get { return wheres.Count; }
      }

      internal bool EnableBracket { get; set; }

      internal Context Context { get; set; }

      public Condition(Select select)
      {
        this.select = select;
      }

      public Condition AddOr(Condition where)
      {
        this.AddWhere(where, Logical.Or);
        return this;
      }

      public Condition AddOr(Object column, Object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.Or, comparison);
        return this;
      }

      public Condition Add(Condition where)
      {
        this.AddWhere(where, Logical.And);
        return this;
      }

      public Condition Add(Object column, Object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.And, comparison);
        return this;
      }

      private Condition AddWhere(Condition where, Logical logical)
      {
        where.EnableBracket = true;
        this.Add(new Holder
        {
          Value = where,
          Logical = logical
        });
        return this;
      }

      private Condition AddColumn(Object columnName, Object value, Logical logical, Comparison comparison)
      {
        var column = new Column(columnName);
        column.Context = this.Context;

        this.Add(new Holder
        {
          Column = column,
          Logical = logical,
          Comparison = comparison,
          Value = value
        });

        return this;
      }

      private void Add(Holder holder)
      {
        if(holder.Logical == Logical.Or && this.wheres.Count == 0)
        {
          //一番最初のWhereがOrで足されたということは何かプログラマーの意図と違うことが起こってるはず。
          throw new Exception("Illegal logical operation for the first where condition `"+holder.Logical.SqlString()+"`");
        }

        this.wheres.Add(holder);
      }

      internal string Build(DbParameterCollection parameters, Counter condCount)
      {
        string whereString = "";

        wheres.ForEach(holder =>
        {
          if (whereString.Length > 0)
          {
            whereString += holder.Logical.SqlString();
          }

          if (holder.Value is Condition)
          {
            Condition where = holder.Value as Condition;
            whereString += where.Build(parameters, condCount);
          }
          else
          {
            whereString += this.BuildValueConditionString(parameters, holder, condCount);
          }
        });

        if (this.EnableBracket)
        {
          whereString = "(" + whereString + ")";
        }

        return whereString;
      }

      private string BuildValueConditionString(DbParameterCollection parameters, Holder cond, Counter condCount)
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

        this.select.ContextList.ForEach(context => {
          leftHand = leftHand.Replace("{"+context.Name+"}", this.select.Adapter.QuoteIdentifier(context.Name));
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
