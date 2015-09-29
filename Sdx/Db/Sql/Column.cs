using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
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
        throw new NotSupportedException("columnName support only Sdx.Db.Query.Expr or string, " + columnName.GetType() + " given.");
      }
    }

    public Column(object columnName, string contextName) : this(columnName)
    {
      this.ContextName = contextName;
    }

    public string Alias { get; internal set; }

    public string ContextName { get; internal set; }

    public Order Order { get; internal set; }

    public object Target
    {
      get { return this.name; }
    }

    public string Name
    {
      get
      {
        if(this.Alias != null)
        {
          return this.Alias;
        }

        return this.name.ToString();
      }
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
      if(this.ContextName != null)
      {
        sql = db.QuoteIdentifier(this.ContextName) + "." + this.QuotedName(db);
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
