using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Sdx.Db
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
    private SelectTable from;
    private List<SelectTable> joins;

    public Select(Factory factory)
    {
      this.joins = new List<SelectTable>();
      this.factory = factory;
      this.JoinOrder = JoinOrder.InnerFront;
    }

    internal Factory Factory
    {
      get { return this.factory; }
    }

    internal List<SelectTable> Joins
    {
      get { return this.joins; }
    }

    public JoinOrder JoinOrder { get; set; }

    public SelectTable From(string tableName, string alias = null)
    {
      SelectTable from = new SelectTable(this);
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
      List<SelectTable> joins;
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

    public SelectTable Table(string name)
    {
      if(this.from.Name == name)
      {
        return this.from;
      }

      foreach(SelectTable from in this.joins)
      {
        if(from.Name == name)
        {
          return from;
        }
      }

      throw new Exception("Missing " + name + " table current context.");
    }
  }
}
