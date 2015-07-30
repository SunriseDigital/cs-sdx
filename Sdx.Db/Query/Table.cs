﻿using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Query
{
  public class Table
  {
    private Select select;

    public Table(Select select)
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

    private Table AddJoin(object table, JoinType joinType, string condition, string alias = null)
    {
      Table joinTable = new Table(this.select);

      joinTable.ParentTable = this;
      joinTable.Target = table;
      joinTable.Alias = alias;
      joinTable.JoinCondition = condition;
      joinTable.JoinType = joinType;

      this.select.RemoveTable(joinTable.Name);

      this.select.TableList.Add(joinTable);
      return joinTable;
    }

    public Table InnerJoin(Select select, string condition, string alias = null)
    {
      return this.AddJoin(select, JoinType.Inner, condition, alias);
    }

    public Table LeftJoin(Select table, string condition, string alias = null)
    {
      return this.AddJoin(table, JoinType.Left, condition, alias);
    }

    public Table InnerJoin(Expr table, string condition, string alias = null)
    {
      return this.AddJoin(table, JoinType.Inner, condition, alias);
    }

    public Table LeftJoin(Expr table, string condition, string alias = null)
    {
      return this.AddJoin(table, JoinType.Left, condition, alias);
    }

    public Table InnerJoin(string table, string condition, string alias = null)
    {
      return this.AddJoin(table, JoinType.Inner, condition, alias);
    }

    public Table LeftJoin(string table, string condition, string alias = null)
    {
      return this.AddJoin(table, JoinType.Left, condition, alias);
    }

    internal Table ParentTable { get; private set; }

    internal string JoinCondition { get; private set; }

    internal JoinType JoinType { get; set; }

    public Table ClearColumns()
    {
      this.select.ClearColumns(this);
      return this;
    }

    public Table Columns(params String[] columns)
    {
      foreach (var column in columns)
      {
        this.Column(column);
      }
      return this;
    }

    public Table Column(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      column.Table = this;
      this.select.ColumnList.Add(column);
      return this;
    }

    public Table Columns(Dictionary<string, object> columns)
    {
      foreach(var column in columns)
      {
        this.Column(column.Value, column.Key);
      }

      return this;
    }

    public string AppendAlias(string column)
    {
      return this.select.Factory.QuoteIdentifier(this.Name) + "." + this.select.Factory.QuoteIdentifier(column);
    }

    public Where Where
    {
      get
      {
        Where where = this.select.Where;
        where.Table = this;
        return where;
        //ここは下記のようにするとTableの代入ができません。
        //this.select.Where.Table = this.Name;
        //return this.select.Where;
        //Select.Writeが下記のような実装になっているからです。
        //public Where Where
        //{
        //  get
        //  {
        //    this.where.Table = null;
        //    return this.where;
        //  }
        //}
      }
    }

    public Where Having
    {
      get
      {
        Where having = this.select.Having;
        having.Table = this;
        return having;
      }
    }

    public Table Group(object columnName)
    {
      var column = new Column(columnName);
      column.Table = this;
      this.select.GroupList.Add(column);
      return this;
    }

    public Table Order(object columnName, Order order)
    {
      var column = new Column(columnName);
      column.Table = this;
      column.Order = order;
      this.select.OrderList.Add(column);

      return this;
    }
  }
}
