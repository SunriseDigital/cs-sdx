using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db.Query
{
  public class Table
  {
    private Select select;

    private Sdx.Db.Table ormTable;

    public Table(Select select)
    {
      this.select = select;
    }

    public object Name { get; internal set; }

    public string Alias { get; internal set; }

    public string ContextName
    {
      get 
      {
        if (this.Alias != null)
        {
          return this.Alias;
        }

        return this.Name.ToString(); 
      }
    }

    private Table AddJoin(object table, JoinType joinType, string condition, string alias = null)
    {
      Table joinTable = new Table(this.select);

      joinTable.ParentTable = this;
      joinTable.Name = table;
      joinTable.Alias = alias;
      joinTable.JoinCondition = condition;
      joinTable.JoinType = joinType;

      this.select.RemoveTable(joinTable.ContextName);

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
      return this.select.Adapter.QuoteIdentifier(this.ContextName) + "." + this.select.Adapter.QuoteIdentifier(column);
    }

    public Condition Where
    {
      get
      {
        Condition where = this.select.Where;
        where.Table = this;
        return where;
        //ここは下記のようにするとTableの代入ができません。
        //this.select.Condition.Table = this.ContextName;
        //return this.select.Condition;
        //Select.Writeが下記のような実装になっているからです。
        //public Condition Condition
        //{
        //  get
        //  {
        //    this.where.Table = null;
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

    internal void setOrmTable(Sdx.Db.Table table)
    {
      this.ormTable = table;
    }

    public Sdx.Db.Table Orm { get { return this.ormTable; } }
  }
}
