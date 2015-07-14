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

  static class JoinTypeExt
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

    public JoinOrder JoinOrder { get; set; }

    public Table From(string tableName, string alias = null)
    {
      Table from = new Table(this);
      from.TableName = tableName;
      from.Alias = alias;
      from.JoinType = JoinType.From;

      this.joins.Add(from);

      return from;
    }

    public DbCommand Build()
    {
      DbCommand command = this.factory.CreateCommand();

      command.CommandText = "SELECT";

      //カラムを組み立てる
      var columnString = "";

      //selectのカラムを追加
      if(this.columns.Count > 0)
      {
        columnString += " " + this.BuildColumsString();
      }

      //from/joinしてるテーブルのカラムを追加
      this.joins.ForEach(sTable => {
        if(sTable.Columns.Count > 0)
        {
          if (columnString.Length > 0)
          {
            columnString += ",";
          }

          columnString += " " + sTable.BuildColumsString();
        }
      });

      command.CommandText += columnString + " FROM ";

      //FROMを追加
      string formString = "";
      foreach (Table table in this.joins.Where(t => t.JoinType == JoinType.From))
      {
        if (formString != "")
        {
          formString += ", ";
        }

        formString += table.BuildTableString();
      }

      command.CommandText += formString;

      if (this.JoinOrder == JoinOrder.InnerFront)
      {
        foreach(var table in this.joins.Where(t => t.JoinType == JoinType.Inner))
        {
          this.appendJoinString(command, table);
        }

        foreach (var table in this.joins.Where(t => t.JoinType == JoinType.Left))
        {
          this.appendJoinString(command, table);
        }
      }
      else
      {
        foreach (var table in this.joins.Where(t => t.JoinType == JoinType.Inner || t.JoinType == JoinType.Left))
        {
          this.appendJoinString(command, table);
        }
      }

      if(this.where.Count > 0)
      {
        command.CommandText += " WHERE ";
        this.where.Build(command);
      }

      return command;
    }

    private void appendJoinString(DbCommand command, Table table)
    {
      command.CommandText += " "
        + table.JoinType.SqlString()
        + " "
        + this.Factory.QuoteIdentifier(table.TableName);

      if (table.Alias != null)
      {
        command.CommandText += " AS " + this.Factory.QuoteIdentifier(table.Name);
      }

      if (table.JoinCondition != null)
      {
        command.CommandText += " ON "
          + String.Format(
            table.JoinCondition,
            this.Factory.QuoteIdentifier(table.ParentTable.Name),
            this.Factory.QuoteIdentifier(table.Name)
          );
      }
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
        return this.where;
      }
    }
  }
}
