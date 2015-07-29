using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Query
{
  public class Column
  {
    private object name;
    public Column(object columnName)
    {
      if(columnName is Expr)
      {
        name = columnName;
      }
      else if( columnName is string)
      {
        var strName = columnName as string;
        if (strName == "*")
        {
          name = Expr.Wrap(strName);
        }
        else
        {
          name = columnName;
        }
      }
      else
      {
        throw new Exception("columnName support only Sdx.Db.Query.Expr or string, " + columnName.GetType() + " given.");
      }
    }

    public Column(Expr columnName)
    {
      name = columnName;
    }

    public string Alias { get; set; }

    public Table Table { get; set; }

    public object Name
    {
      get { return this.name; }
    }

    private string QuotedName(Factory factory)
    {
      if(this.name is Expr)
      {
        return factory.QuoteIdentifier(this.name as Expr);
      }
      else
      {
        return factory.QuoteIdentifier(this.name as String);
      }
    }

    internal string Build(Factory factory)
    {
      var sql = "";
      if(this.Table != null)
      {
        sql = factory.QuoteIdentifier(this.Table.Name) + "." + this.QuotedName(factory);
      }
      else
      {
        sql = this.QuotedName(factory);
      }

      if(this.Alias != null)
      {
        sql += " AS " + factory.QuoteIdentifier(this.Alias);
      }

      return sql;
    }
  }
}
