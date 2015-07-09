using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Db
{
  public class SelectColumn
  {
    private object name;
    public SelectColumn(string columnName)
    {
      if(columnName == "*")
      {
        name = new Expr(columnName);
      }
      else
      {
        name = columnName;
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
