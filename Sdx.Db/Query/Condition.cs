﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
    public class Condition
    {
      private enum TableDetectMode
      {
        Column, //Sdx.Db.Query.Column.Context
        Left,   //{0}
        Right   //{1}
      }

      private class Holder
      {
        public Comparison Comparison { get; set; }
        public Logical Logical { get; set; }
        public Column Column { get; set; }
        public object Value { get; set; }
        public TableDetectMode TableDetectMode { get; set; }
      }

      private List<Holder> wheres = new List<Holder>();

      public Condition(string baseCond)
      {
        this.Base = baseCond;
      }

      public Condition()
      {
        
      }

      internal int Count
      {
        get { return wheres.Count; }
      }

      internal bool EnableBracket { get; set; }

      internal Context Context { get; set; }

      public Condition AddOr(Condition where)
      {
        this.AddWhere(where, Logical.Or);
        return this;
      }

      public Condition AddOr(Object column, Object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.Or, comparison, TableDetectMode.Column);
        return this;
      }

      public Condition Add(Condition where)
      {
        this.AddWhere(where, Logical.And);
        return this;
      }

      public Condition Add(Object column, Object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.And, comparison, TableDetectMode.Column);
        return this;
      }

      public Condition AddRight(object column, object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.And, comparison, TableDetectMode.Right);
        return this;
      }

      public Condition AddLeft(object column, object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.And, comparison, TableDetectMode.Left);
        return this;
      }

      public Condition AddRightOr(object column, object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.Or, comparison, TableDetectMode.Right);
        return this;
      }

      public Condition AddLeftOr(object column, object value, Comparison comparison = Comparison.Equal)
      {
        this.AddColumn(column, value, Logical.Or, comparison, TableDetectMode.Left);
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

      private void AddColumn(Object columnName, Object value, Logical logical, Comparison comparison, TableDetectMode mode)
      {
        var column = new Column(columnName);

        if(mode == TableDetectMode.Column)
        {
          column.Context = this.Context;
        }
        
        this.Add(new Holder
        {
          Column = column,
          Logical = logical,
          Comparison = comparison,
          Value = value,
          TableDetectMode = mode
        });
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

      /// <summary>
      /// ユニットテスト用。普通は使用しません。
      /// </summary>
      /// <param name="parameters"></param>
      /// <returns></returns>
      public string Build(Select select, DbParameterCollection parameters)
      {
        var counter = new Counter();
        return this.Build(select, parameters, counter);
      }

      internal string Build(Select select, DbParameterCollection parameters, Counter condCount)
      {
        string whereString = "";

        if(this.Base != null)
        {
          whereString = this.Base;
        }

        wheres.ForEach(holder =>
        {
          if (whereString.Length > 0)
          {
            whereString += holder.Logical.SqlString();
          }

          if (holder.Value is Condition)
          {
            Condition where = holder.Value as Condition;
            whereString += where.Build(select, parameters, condCount);
          }
          else
          {
            whereString += this.BuildValueConditionString(select, parameters, holder, condCount);
          }
        });

        if (this.EnableBracket)
        {
          whereString = "(" + whereString + ")";
        }

        return whereString;
      }

      private string BuildValueConditionString(Select select, DbParameterCollection parameters, Holder cond, Counter condCount)
      {
        string rightHand;
        if (cond.Value is Expr)
        {
          rightHand = cond.Value.ToString();
          condCount.Incr();
        }
        else if (cond.Value is Select)
        {
          Select sub = (Select)cond.Value;
          rightHand = "(" + sub.BuildSelectString(parameters, condCount) + ")";
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
            parameters.Add(select.Adapter.CreateParameter(holder, value.ToString()));
            inCond += holder;
            condCount.Incr();
          }

          rightHand = "(" + inCond + ")";
        }
        else
        {
          rightHand = "@" + condCount.Value;
          parameters.Add(select.Adapter.CreateParameter(rightHand, cond.Value.ToString()));
          condCount.Incr();
        }

        var leftHand = cond.Column.Build(select.Adapter);

        select.ContextList.ForEach(context =>
        {
          leftHand = leftHand.Replace("{" + context.Name + "}", select.Adapter.QuoteIdentifier(context.Name));
        });

        if (cond.TableDetectMode == TableDetectMode.Left)
        {
          leftHand = "{0}." + leftHand;
        }
        else if (cond.TableDetectMode == TableDetectMode.Right)
        {
          leftHand = "{1}." + leftHand;
        }

        return String.Format(
          "{0}{1}{2}",
          leftHand,
          cond.Comparison.SqlString(),
          rightHand
        );
      }

      public string Base { get; set; }
    }
}
