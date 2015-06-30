using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Sdx.Db
{
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

    public Select From(string tableName, string alias = null)
    {
      From from = new From(this.builder);
      from.TableName = tableName;
      from.Alias = alias;

      this.from = from;

      return this;
    }

    public System.Data.Common.DbCommand build()
    {
      DbCommand command = this.factory.CreateCommand();

      command.CommandText = "SELECT " 
        + this.from.BuildColumsString()
        + " FROM " +this.from.BuildTableString();

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
