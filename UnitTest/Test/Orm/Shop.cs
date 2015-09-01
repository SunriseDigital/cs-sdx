using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class Shop : Sdx.Db.Record
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Shop()
    {
      Meta = Test.Orm.Table.Shop.Meta;
    }
  }
}
      