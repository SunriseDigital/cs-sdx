using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Db
{
  public class Table : Sdx.Db.Table
  {
    public override Sdx.Db.Table SelectDefaultOrder(Sdx.Db.Sql.Select select)
    {
      select.AddOrder("id", Sdx.Db.Sql.Order.ASC);
      return this;
    }
  }
}
