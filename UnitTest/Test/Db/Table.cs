using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Db
{
  public class Table : Sdx.Db.Table
  {
    public override Sdx.Db.Table SelectDefaultOrder(Sdx.Db.Sql.Select select, Sdx.Db.Connection conn = null)
    {
      var context = FindSelfContext(select);
      
      if(OwnMeta.HasColumn("sequence"))
      {
        context.AddOrder("sequence", Sdx.Db.Sql.Order.DESC);
      }
      else if(OwnMeta.HasColumn("id"))
      {
        context.AddOrder("id", Sdx.Db.Sql.Order.ASC);
      }
      
      return this;
    }
  }
}
