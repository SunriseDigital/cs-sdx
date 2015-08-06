﻿using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Query
{
  public class Context
  {
    private Select select;

    private Sdx.Db.Table table;

    public Context(Select select)
    {
      this.select = select;
    }

    public object Target { get; internal set; }

    public string Alias { get; internal set; }

    public string Name
    {
      get 
      {
        if (this.Alias != null)
        {
          return this.Alias;
        }

        return this.Target.ToString(); 
      }
    }

    private Context AddJoin(object target, JoinType joinType, Condition condition, string alias = null)
    {
      Context joinContext = new Context(this.select);

      joinContext.ParentContext = this;
      joinContext.Target = target;
      joinContext.Alias = alias;
      joinContext.JoinCondition = condition;
      joinContext.JoinType = joinType;

      this.select.RemoveContext(joinContext.Name);

      this.select.ContextList.Add(joinContext);
      return joinContext;
    }

    public Context InnerJoin(Sdx.Db.Table target, Condition condition = null, string alias = null)
    {
      var context = this.AddJoin(target.Meta.Name, JoinType.Inner, condition, alias);

      if(condition == null)
      {
        var relation = target.Meta.Relations[this.Name];
        context.JoinCondition = new Condition(relation.JoinCondition);
      }

      context.Table = target;
      target.ContextName = context.Name;
      target.Select = this.select;
      return context;
    }

    public Context InnerJoin(Sdx.Db.Table target, string alias)
    {
      return this.InnerJoin(target, null, alias);
    }

    public Context LeftJoin(Sdx.Db.Table target, Condition condition = null, string alias = null)
    {
      var context = this.AddJoin(target.Meta.Name, JoinType.Left, condition, alias);

      if (condition == null)
      {
        var relation = target.Meta.Relations[this.Name];
        context.JoinCondition = new Condition(relation.JoinCondition);
      }

      context.Table = target;
      target.ContextName = context.Name;
      target.Select = this.select;
      return context;
    }

    public Context LeftJoin(Sdx.Db.Table target, string alias)
    {
      return this.LeftJoin(target, null, alias);
    }


    public Context InnerJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(Select target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context InnerJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(Expr target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    public Context InnerJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Inner, condition, alias);
    }

    public Context LeftJoin(string target, Condition condition, string alias = null)
    {
      return this.AddJoin(target, JoinType.Left, condition, alias);
    }

    internal Context ParentContext { get; private set; }

    internal Condition JoinCondition { get; private set; }

    internal JoinType JoinType { get; set; }

    public Context ClearColumns()
    {
      this.select.ClearColumns(this.Name);
      return this;
    }

    public Context Columns(params String[] columns)
    {
      foreach (var column in columns)
      {
        this.Column(column);
      }
      return this;
    }

    public Context Column(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      column.ContextName = this.Name;
      this.select.ColumnList.Add(column);
      return this;
    }

    public Context Columns(Dictionary<string, object> columns)
    {
      foreach(var column in columns)
      {
        this.Column(column.Value, column.Key);
      }

      return this;
    }

    public string AppendAlias(string column)
    {
      return this.select.Adapter.QuoteIdentifier(this.Name) + "." + this.select.Adapter.QuoteIdentifier(column);
    }

    public Condition Where
    {
      get
      {
        Condition where = this.select.Where;
        where.Context = this;
        return where;
        //ここは下記のようにするとContextの代入ができません。
        //this.select.Condition.Context = this;
        //return this.select.Condition;
        //Select.Writeが下記のような実装になっているからです。
        //public Condition Condition
        //{
        //  get
        //  {
        //    this.where.Context = null;
        //    return this.where;
        //  }
        //}
      }
    }

    public Condition Having
    {
      get
      {
        Condition having = this.select.Having;
        having.Context = this;
        return having;
      }
    }

    public Context Group(object columnName)
    {
      var column = new Column(columnName);
      column.ContextName = this.Name;
      this.select.GroupList.Add(column);
      return this;
    }

    public Context Order(object columnName, Order order)
    {
      var column = new Column(columnName);
      column.ContextName = this.Name;
      column.Order = order;
      this.select.OrderList.Add(column);

      return this;
    }

    public Sdx.Db.Table Table 
    {
      get 
      {
        return this.table;
      }

      internal set
      {
        this.table = value;
      }
    }
  }
}