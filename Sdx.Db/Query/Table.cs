﻿using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Query
{
  public class Table
  {
    private Select select;

    private List<Column> columns = new List<Column>();

    public Table(Select select)
    {
      this.select = select;
    }

    public object TableName { get; internal set; }

    public string Alias { get; internal set; }

    public List<Column> Columns
    {
      get { return this.columns; }
    }

    public string Name
    {
      get 
      {
        if (this.Alias != null)
        {
          return this.Alias;
        }

        return this.TableName.ToString(); 
      
      }
    }

    internal string BuildColumsString()
    {
      if (this.columns.Count == 0 && this.ParentTable == null)
      {
        //TODO これが意図したときに呼ばれてない気がする。後、これは例外じゃ無いほうがいいのでは？
        throw new Exception("Column is empty.");
      }

      var result = "";
      this.columns.ForEach((column) =>
      {
        if (result.Length > 0)
        {
          result += ", ";
        }

        result += this.select.Factory.QuoteIdentifier(this.Name) + ".";

        result += this.select.Factory.QuoteIdentifier(column);

        if(column.Alias != null)
        {
          result += " AS " + this.select.Factory.QuoteIdentifier(column.Alias);
        }
        
      });

      return result;
    }

    internal string BuildTableString()
    {
      var result = this.select.Factory.QuoteIdentifier(this);
      if (this.Alias != null)
      {
        result += " AS " + this.select.Factory.QuoteIdentifier(this.Alias);
      }
      return result;
    }

    private Table AddJoin(object table, JoinType joinType, string condition, string alias = null)
    {
      Table joinTable = new Table(this.select);

      joinTable.ParentTable = this;
      joinTable.TableName = table;
      joinTable.Alias = alias;
      joinTable.JoinCondition = condition;
      joinTable.JoinType = joinType;

      this.select.Remove(joinTable.Name);

      this.select.Joins.Add(joinTable);
      return joinTable;
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

    public Table ParentTable { get; private set; }

    public string JoinCondition { get; private set; }

    public JoinType JoinType { get; internal set; }

    public Table ClearColumns()
    {
      this.Columns.Clear();
      return this;
    }

    public Table SetColumns(params String[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Table AddColumns(params String[] columns)
    {
      foreach (var column in columns)
      {
        this.AddColumn(column);
      }
      return this;
    }

    public Table AddColumn(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      this.columns.Add(column);
      return this;
    }

    public Table AddColumns(Dictionary<string, object> columns)
    {
      foreach(var column in columns)
      {
        this.AddColumn(column.Value, column.Key);
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
        where.Table = this.Name;
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
  }
}
