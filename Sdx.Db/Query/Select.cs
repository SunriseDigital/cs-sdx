using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
  public class Select
  {
    private Factory factory;
    private List<Table> joins = new List<Table>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private List<OrderBy> orders = new List<OrderBy>();
    private Where where;
    private Where having;
    private int limit = -1;
    private int offset = -1;

    private class OrderBy
    {
      public Column Column { get; set; }
      public Order Order { get; set; }
    }

    public Select(Factory factory)
    {
      this.factory = factory;
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Where(this);
      this.having = new Where(this);
    }

    internal Factory Factory
    {
      get { return this.factory; }
    }

    internal List<Table> Joins
    {
      get { return this.joins; }
    }

    internal List<Column> Groups
    {
      get { return this.groups; }
    }

    public JoinOrder JoinOrder { get; set; }

    public Table From(object tableName, string alias = null)
    {
      Table from = new Table(this);
      from.Target = tableName;
      from.Alias = alias;
      from.JoinType = JoinType.From;

      this.joins.Add(from);

      return from;
    }

    internal string BuildSelectString(DbParameterCollection parameters, Counter condCount)
    {
      string selectString = "SELECT ";

      //カラムを組み立てる
      selectString += this.BuildColumsString() + " FROM ";

      //FROMを追加
      var fromString = "";
      foreach (Table table in this.joins.Where(t => t.JoinType == JoinType.From))
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
        foreach (var table in this.joins.Where(t => t.JoinType == JoinType.Inner))
        {
          selectString += this.buildJoinString(table, parameters, condCount);
        }

        foreach (var table in this.joins.Where(t => t.JoinType == JoinType.Left))
        {
          selectString += this.buildJoinString(table, parameters, condCount);
        }
      }
      else
      {
        foreach (var table in this.joins.Where(t => t.JoinType == JoinType.Inner || t.JoinType == JoinType.Left))
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
      if (this.Groups.Count > 0)
      {
        var groupString = "";
        this.Groups.ForEach(column =>
        {
          if(groupString != "")
          {
            groupString += ", ";
          }

          groupString += column.Build(this.factory);
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
        this.orders.ForEach(orderBy => { 
          if(orderString.Length > 0)
          {
            orderString += ", ";
          }
          orderString += orderBy.Column.Build(this.factory) + " " + orderBy.Order.SqlString();
        });

        selectString += " ORDER BY " + orderString;
      }

      //LIMIT/OFFSET
      if(this.limit > -1)
      {
        selectString = this.factory.AppendLimitQuery(selectString, this.limit, this.offset);
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

      if (table.Target is Select)
      {
        Select select = table.Target as Select;
        string subquery = select.BuildSelectString(parameters, condCount);
        joinString += "(" + subquery + ")";
      }
      else
      {
        joinString += this.Factory.QuoteIdentifier(table.Target);
      }

      if (table.Alias != null)
      {
        joinString += " AS " + this.Factory.QuoteIdentifier(table.Name);
      }

      if (table.JoinCondition != null)
      {
        joinString += " ON "
          + String.Format(
            table.JoinCondition,
            this.Factory.QuoteIdentifier(table.ParentTable.Name),
            this.Factory.QuoteIdentifier(table.Name)
          );
      }

      return joinString;
    }

    public DbCommand Build()
    {
      DbCommand command = this.factory.CreateCommand();
      var condCount = new Counter();
      command.CommandText = this.BuildSelectString(command.Parameters, condCount);

      return command;
    }

    public Table Table(string name)
    {
      foreach (Table table in this.joins)
      {
        if (table.Name == name)
        {
          return table;
        }
      }

      throw new Exception("Missing " + name + " table current context.");
    }

    public List<Column> Columns
    {
      get { return this.columns; }
    }

    public Select ClearColumns(Table table = null)
    {
      if (table == null)
      {
        this.columns.Clear();
      }
      else
      {
        this.columns.RemoveAll(column => column.Table != null && column.Table.Name == table.Name);
      }
      
      return this;
    }

    public Select SetColumns(params String[] columns)
    {
      this.ClearColumns();
      this.AddColumns(columns);
      return this;
    }

    public Select AddColumns(params String[] columns)
    {
      foreach (var column in columns)
      {
        this.AddColumn(column);
      }
      return this;
    }


    public Select AddColumns(Dictionary<string, object> columns)
    {
      foreach (var column in columns)
      {
        this.AddColumn(column.Value, column.Key);
      }

      return this;
    }

    public Select AddColumn(object columnName, string alias = null)
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

        result += column.Build(this.factory);
      });

      return result;
    }

    public Select Remove(string tableName)
    {
      int findIndex = this.joins.FindIndex(jt =>
      {
        return jt.Name == tableName;
      });

      if (findIndex != -1)
      {
        this.ClearColumns(this.joins[findIndex]);
        this.joins.RemoveAt(findIndex);
      }

      return this;
    }

    public Where Where
    {
      get
      {
        this.where.Table = null;
        return this.where;
      }
    }

    public Where CreateWhere()
    {
      return new Where(this);
    }

    public Expr Expr(string str)
    {
      return Sdx.Db.Query.Expr.Wrap(str);
    }

    public Select Group(object columnName)
    {
      var column = new Column(columnName);
      this.groups.Add(column);
      return this;
    }

    public Where Having 
    {
      get
      {
        this.having.Table = null;
        return this.having;
      }
    }

    public Select Limit(int limit, int offset = 0)
    {
      this.limit = limit;
      this.offset = offset;
      return this;
    }

    public void Order(object columnName, Order order)
    {
      var column = new Column(columnName);
      orders.Add(new OrderBy()
      {
        Column = column,
        Order = order
      });
    }
  }
}
