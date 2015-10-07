using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db.Sql
{
  public class Column
  {
    private object target;

    public string Alias { get; internal set; }

    internal string ContextName { get; set; }

    internal Order Order { get; set; }

    internal object Value { get; set; }

    public Column(object columnName)
    {
      if(columnName is Expr)
      {
        target = columnName;
      }
      else if( columnName is string)
      {
        var strName = columnName as string;
        if (strName == "*")
        {
          target = Expr.Wrap(strName);
        }
        else
        {
          target = columnName;
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

    public object Target
    {
      get { return this.target; }
    }

    public string Name
    {
      get
      {
        if(this.Alias != null)
        {
          return this.Alias;
        }

        return this.target.ToString();
      }
    }

    private string QuotedName(Adapter db)
    {
      if(this.target is Expr)
      {
        return db.QuoteIdentifier(this.target as Expr);
      }
      else
      {
        return db.QuoteIdentifier(this.target as String);
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
