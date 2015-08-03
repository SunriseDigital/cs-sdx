﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
  public class Select
  {
    private Adapter adapter;
    private List<Table> tables = new List<Table>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private List<Column> orders = new List<Column>();
    private Condition where;
    private Condition having;

    internal Select(Adapter adapter)
    {
      this.adapter = adapter;
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Condition(this);
      this.having = new Condition(this);

      //intは0で初期化されてしまうのでセットされていない状態を識別するため（`LIMIT 0`を可能にするため）-1をセット
      this.Limit = -1;
    }

    internal Adapter Adapter
    {
      get { return this.adapter; }
    }

    internal List<Table> TableList
    {
      get { return this.tables; }
    }

    internal List<Column> GroupList
    {
      get { return this.groups; }
    }

    internal List<Column> OrderList
    {
      get { return this.orders; }
    }

    internal List<Column> ColumnList
    {
      get { return this.columns; }
    }

    public JoinOrder JoinOrder { get; set; }

    /// <summary>
    /// From句を追加。繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="contextName">Sdx.Adapter.Query.Expr|String</param>
    /// <param contextName="alias"></param>
    /// <returns></returns>
    public Table From(object tableName, string alias = null)
    {
      Table from = new Table(this);
      from.Name = tableName;
      from.Alias = alias;
      from.JoinType = JoinType.From;

      this.tables.Add(from);

      return from;
    }

    internal string BuildSelectString(DbParameterCollection parameters, Counter condCount)
    {
      string selectString = "SELECT ";

      //カラムを組み立てる
      selectString += this.BuildColumsString() + " FROM ";

      //FROMを追加
      var fromString = "";
      foreach (Table table in this.tables.Where(t => t.JoinType == JoinType.From))
      {
        if (fromString != "")
        {
          fromString += ", ";
        }
        fromString += this.buildJoinString(table, parameters, condCount);
      }

      selectString += fromString;

      //JOIN
      if (this.JoinOrder == JoinOrder.InnerFront)
      {
        foreach (var table in this.tables.Where(t => t.JoinType == JoinType.Inner))
        {
          selectString += this.buildJoinString(table, parameters, condCount);
        }

        foreach (var table in this.tables.Where(t => t.JoinType == JoinType.Left))
        {
          selectString += this.buildJoinString(table, parameters, condCount);
        }
      }
      else
      {
        foreach (var table in this.tables.Where(t => t.JoinType == JoinType.Inner || t.JoinType == JoinType.Left))
        {
          selectString += this.buildJoinString(table, parameters, condCount);
        }
      }

      if (this.where.Count > 0)
      {
        selectString += " WHERE ";
        selectString += this.where.Build(parameters, condCount);
      }

      //GROUP
      if (this.GroupList.Count > 0)
      {
        var groupString = "";
        this.GroupList.ForEach(column =>
        {
          if(groupString != "")
          {
            groupString += ", ";
          }

          groupString += column.Build(this.adapter);
        });

        selectString += " GROUP BY " + groupString;
      }

      //Having
      if(this.having.Count > 0)
      {
        selectString += " HAVING ";
        selectString += this.having.Build(parameters, condCount);
      }

      //ORDER
      if(this.orders.Count > 0)
      {
        var orderString = "";
        this.orders.ForEach(column => { 
          if(orderString.Length > 0)
          {
            orderString += ", ";
          }
          orderString += column.Build(this.adapter) + " " + column.Order.SqlString();
        });

        selectString += " ORDER BY " + orderString;
      }

      //LIMIT/OFFSET
      if(this.Limit > -1)
      {
        selectString = this.adapter.AppendLimitQuery(selectString, this.Limit, this.Offset);
      }

      return selectString;
    }

    private string buildJoinString(Table table, DbParameterCollection parameters, Counter condCount)
    {
      string joinString = "";

      if (table.JoinType != JoinType.From)
      {
        joinString += " " + table.JoinType.SqlString() + " ";
      }

      if (table.Name is Select)
      {
        Select select = table.Name as Select;
        string subquery = select.BuildSelectString(parameters, condCount);
        joinString += "(" + subquery + ")";
      }
      else
      {
        joinString += this.Adapter.QuoteIdentifier(table.Name);
      }

      if (table.Alias != null)
      {
        joinString += " AS " + this.Adapter.QuoteIdentifier(table.ContextName);
      }

      if (table.JoinCondition != null)
      {
        joinString += " ON "
          + String.Format(
            table.JoinCondition,
            this.Adapter.QuoteIdentifier(table.ParentTable.ContextName),
            this.Adapter.QuoteIdentifier(table.ContextName)
          );
      }

      return joinString;
    }

    public DbCommand Build()
    {
      DbCommand command = this.adapter.CreateCommand();
      var condCount = new Counter();
      command.CommandText = this.BuildSelectString(command.Parameters, condCount);

      return command;
    }

    /// <summary>
    /// From及びJoinしたテーブルの中からcontextNameのテーブルを探し返す
    /// </summary>
    /// <param name="contextName"></param>
    /// <returns></returns>
    public Table Table(string contextName)
    {
      foreach (Table table in this.tables)
      {
        if (table.ContextName == contextName)
        {
          return table;
        }
      }

      throw new Exception("Missing " + contextName + " table current context.");
    }

    /// <summary>
    /// 追加したカラムをクリアする。
    /// </summary>
    /// <param contextName="table">Tableを渡すとそのテーブルのカラムのみをクリアします。</param>
    /// <returns></returns>
    public Select ClearColumns(Table table = null)
    {
      if (table == null)
      {
        this.columns.Clear();
      }
      else
      {
        this.columns.RemoveAll(column => column.Table != null && column.Table.ContextName == table.ContextName);
      }
      
      return this;
    }

    /// <summary>
    /// エイリアスの付与はできません。
    /// </summary>
    /// <param contextName="columns">Sdx.Adapter.Query.Expr[]|String[]</param>
    /// <returns></returns>
    public Select Columns(params object[] columns)
    {
      foreach (var column in columns)
      {
        this.Column(column);
      }
      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param contextName="columns">エイリアスがKeyでカラムがValueのDictionary</param>
    /// <returns></returns>
    public Select Columns(Dictionary<string, object> columns)
    {
      foreach (var column in columns)
      {
        this.Column(column.Value, column.Key);
      }

      return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param contextName="columnName">Sdx.Adapter.Query.Expr|String</param>
    /// <param contextName="alias"></param>
    /// <returns></returns>
    public Select Column(object columnName, string alias = null)
    {
      var column = new Column(columnName);
      column.Alias = alias;
      this.columns.Add(column);
      return this;
    }

    internal string BuildColumsString()
    {
      var result = "";
      this.columns.ForEach((column) =>
      {
        if (result.Length > 0)
        {
          result += ", ";
        }

        result += column.Build(this.adapter);
      });

      return result;
    }

    /// <summary>
    /// From及びJoinしたテーブルの中からcontextNameのテーブルを探し削除する
    /// </summary>
    /// <param contextName="contextName"></param>
    /// <returns></returns>
    public Select RemoveTable(string contextName)
    {
      int findIndex = this.tables.FindIndex(jt =>
      {
        return jt.ContextName == contextName;
      });

      if (findIndex != -1)
      {
        this.ClearColumns(this.tables[findIndex]);
        this.tables.RemoveAt(findIndex);
      }

      return this;
    }

    public Condition Where
    {
      get
      {
        this.where.Table = null;
        return this.where;
      }
    }

    public Condition CreateWhere()
    {
      return new Condition(this);
    }

    /// <summary>
    /// 繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="columnName">Sdx.Adapter.Query.Expr|String</param>
    /// <returns></returns>
    public Select Group(object columnName)
    {
      var column = new Column(columnName);
      this.groups.Add(column);
      return this;
    }

    public Condition Having 
    {
      get
      {
        this.having.Table = null;
        return this.having;
      }
    }

    public int Limit { get; set; }

    public int Offset { get; set; }

    /// <summary>
    /// 繰り返しコールすると繰り返し追加します。
    /// </summary>
    /// <param contextName="columnName">Sdx.Adapter.Query.Expr|String</param>
    /// <param contextName="order"></param>
    /// <returns></returns>
    public Select Order(object columnName, Order order)
    {
      var column = new Column(columnName);
      column.Order = order;
      orders.Add(column);

      return this;
    }
  }
}
