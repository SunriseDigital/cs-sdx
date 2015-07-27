using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
  public enum JoinType
  {
    From,
    Inner,
    Left
  };

  public enum JoinOrder
  {
    InnerFront,
    Natural
  }

  public static class SelectEnumExtension
  {
    public static string SqlString(this JoinType gender)
    {
      string[] strings = { "FROM", "INNER JOIN", "LEFT JOIN" };
      return  strings[(int) gender];
    }
  }

  public class Select
  {
    private Factory factory;
    private List<Table> joins = new List<Table>();
    private List<Column> columns = new List<Column>();
    private List<Column> groups = new List<Column>();
    private Where where;

    public Select(Factory factory)
    {
      this.factory = factory;
      this.JoinOrder = JoinOrder.InnerFront;
      this.where = new Where(factory);
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
      string selectString = "SELECT";

      //カラムを組み立てる
      var columnString = "";

      //selectのカラムを追加
      if (this.columns.Count > 0)
      {
        columnString += " " + this.BuildColumsString();
      }

      //from/joinしてるテーブルのカラムを追加
      this.joins.ForEach(sTable =>
      {
        if (sTable.Columns.Count > 0)
        {
          if (columnString.Length > 0)
          {
            columnString += ",";
          }

          columnString += " " + sTable.BuildColumsString();
        }
      });

      selectString += columnString + " FROM ";

      //FROMを追加
      string formString = "";
      foreach (Table table in this.joins.Where(t => t.JoinType == JoinType.From))
      {
        if (formString != "")
        {
          formString += ", ";
        }

        if(table.Target is Select)
        {
          Select select = table.Target as Select;
          formString += "(" + select.BuildSelectString(parameters, condCount) + ")";
        }
        else
        {
          formString += this.Factory.QuoteIdentifier(table);
        }

        if (table.Alias != null)
        {
          formString += " AS " + this.Factory.QuoteIdentifier(table.Alias);
        }
      }

      selectString += formString;

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


      return selectString;
    }

    private string buildJoinString(Table table, DbParameterCollection parameters, Counter condCount)
    {
      string joinString = "";

      if (table.Target is Select)
      {
        Select select = table.Target as Select;
        string subquery = select.BuildSelectString(parameters, condCount);
        joinString += " "
          + table.JoinType.SqlString() + " "
          + "(" + subquery + ")";
      }
      else
      {
        joinString += " "
          + table.JoinType.SqlString() + " "
          + this.Factory.QuoteIdentifier(table);
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

    public Select ClearColumns()
    {
      this.columns.Clear();
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

        result += this.Factory.QuoteIdentifier(column);

        if (column.Alias != null)
        {
          result += " AS " + this.Factory.QuoteIdentifier(column.Alias);
        }

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
      return new Where(this.factory);
    }

    public Expr Expr(string str)
    {
      return new Expr(str);
    }

    public Select Group(string columnName)
    {
      var column = new Column(columnName);
      this.groups.Add(column);
      return this;
    }
  }
}
