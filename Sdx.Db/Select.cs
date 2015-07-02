using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
  enum JoinType
  {
    Inner,
    Left
  };

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
    private DbCommandBuilder builder;
    private From from;
    private List<From> joins;

    public Select(Factory factory)
    {
      this.joins = new List<From>();
      this.factory = factory;
      this.builder = factory.CreateCommandBuilder();
    }

    internal DbCommandBuilder Builder
    {
      get { return this.builder; }
    }

    internal List<From> Joins
    {
      get { return this.joins; }
    }

    public Select From(string tableName, string alias = null)
    {
      From from = new From(this);
      from.TableName = tableName;
      from.Alias = alias;

      this.from = from;

      return this;
    }

    public DbCommand Build()
    {
      DbCommand command = this.factory.CreateCommand();

      command.CommandText = "SELECT "
        + this.from.BuildColumsString();

      this.joins.ForEach(from => {
        command.CommandText += ", " + from.BuildColumsString();
      });

      command.CommandText += " FROM " + this.from.BuildTableString();

      this.joins.ForEach(from => {
        command.CommandText += " "
          + from.JoinType.SqlString() + " " + from.QuotedTableName;

        if(from.Alias != null)
        {
          command.CommandText += " AS " + from.QuotedName;
        }

        if(from.JoinCondition != null)
        {
          command.CommandText += " ON "
            + String.Format(
              from.JoinCondition,
              from.ParentTable.QuotedName,
              from.QuotedName
            );
        }
      });

      return command;
    }

    public From Table(string name)
    {
      if(this.from.Name == name)
      {
        return this.from;
      }

      foreach(From from in this.joins)
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
