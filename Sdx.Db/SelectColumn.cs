using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public class SelectColumn
  {
    private object name;
    public SelectColumn(object columnName)
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
          name = new Expr(strName);
        }
        else
        {
          name = columnName;
        }
      }
      else
      {
        throw new Exception("columnName must be instance of string or Expr");
      }
    }

    public SelectColumn(Expr columnName)
    {
      name = columnName;
    }

    public string Alias { get; set; }

    public object Name
    {
      get { return this.name; }
    }

    public bool isExpr()
    {
      return this.name is Expr;
    }
  }
}
