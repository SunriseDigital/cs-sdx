using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class ShopImageType : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopImageType()
    {
      Meta = Test.Orm.Table.ShopImageType.Meta;
    }
  }
}
