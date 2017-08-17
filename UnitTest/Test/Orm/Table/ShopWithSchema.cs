using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Orm.Table
{
  public class ShopWithSchema : Shop
  {
    public static new Sdx.Db.TableMeta Meta { get; private set; }

    static ShopWithSchema()
    {
      var Relations = CreateRelations();

      Relations["menu"] = new Relation(
        typeof(Test.Orm.Table.MenuWithSchema),
        "id",
        "shop_id"
      );

      Meta = new Sdx.Db.TableMeta(
        "dbo.shop",
        CreateColumns(),
        Relations,
        typeof(Test.Orm.ShopWithSchema),
        typeof(Test.Orm.Table.ShopWithSchema),
        "shop"
      );
    }
  }
}
