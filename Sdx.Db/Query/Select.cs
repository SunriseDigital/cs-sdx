using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db.Query
{
  public enum JoinType
  {
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
      string[] strings = { "INNER JOIN", "LEFT JOIN" };
      return  strings[(int) gender];
    }
  }

  public class Select
  {
    private Factory factory;
    private Table from;
    private List<Table> joins = new List<Table>();
    private List<Column> columns = new List<Column>();

    public Select(Factory factory)
    {
      this.factory = factory;
      this.JoinOrder = JoinOrder.InnerFront;
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

      this.from = from;

      return from;
    }

    public DbCommand Build()
    {
      DbCommand command = this.factory.CreateCommand();

      command.CommandText = "SELECT";

      //カラムを組み立てる
      var columns = "";

      //selectのカラムを追加
      if(this.columns.Count > 0)
      {
        columns += " " + this.BuildColumsString();
      }

      //fromのカラムを追加
      if(this.from.Columns.Count > 0)
      {
        columns += " " + this.from.BuildColumsString();
      }

      //joinしてるテーブルのカラムを追加
      this.joins.ForEach(sTable => {
        if(sTable.Columns.Count > 0)
        {
          if (columns.Length > 0)
          {
            columns += ", ";
          }

          columns += sTable.BuildColumsString();
        }
      });

      //FROMを追加
      command.CommandText += columns + " FROM " + this.from.BuildTableString();

      //JOIN句を組み立てる
      //InnerFrontのときはソートするのでコピーする
      List<Table> joins;
      if (this.JoinOrder == JoinOrder.InnerFront)
      {
        joins = this.joins.OrderBy(table => table.JoinType == JoinType.Left).ToList();
      }
      else
      {
        joins = this.joins;
      }

      joins.ForEach(sTable => {
        command.CommandText += " "
          + sTable.JoinType.SqlString()
          + " "
          + this.Factory.QuoteIdentifier(sTable.TableName);

        if(sTable.Alias != null)
        {
          command.CommandText += " AS " + this.Factory.QuoteIdentifier(sTable.Name);
        }

        if(sTable.JoinCondition != null)
        {
          command.CommandText += " ON "
            + String.Format(
              sTable.JoinCondition,
              this.Factory.QuoteIdentifier(sTable.ParentTable.Name),
              this.Factory.QuoteIdentifier(sTable.Name)
            );
        }
      });

      return command;
    }

    public Table Table(string name)
    {
      if(this.from.Name == name)
      {
        return this.from;
      }

      foreach(Table from in this.joins)
      {
        if(from.Name == name)
        {
          return from;
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
  }
}
