using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Query
{
  internal class Column
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

    public Order Order { get; set; }

    public object Name
    {
      get { return this.name; }
    }

    private string QuotedName(Adapter db)
    {
      if(this.name is Expr)
      {
        return db.QuoteIdentifier(this.name as Expr);
      }
      else
      {
        return db.QuoteIdentifier(this.name as String);
      }
    }

    internal string Build(Adapter db)
    {
      var sql = "";
      if(this.Table != null)
      {
        sql = db.QuoteIdentifier(this.Table.ContextName) + "." + this.QuotedName(db);
      }
      else
      {
        sql = this.QuotedName(db);
      }

      if(this.Alias != null)
      {
        sql += " AS " + db.QuoteIdentifier(this.Alias);
      }

      return sql;
    }
  }
}
