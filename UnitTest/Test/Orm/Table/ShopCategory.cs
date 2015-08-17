using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class ShopCategory : Sdx.Db.Table
  {
    public static new Sdx.Db.TableMeta Meta { get; private set; }

    static ShopCategory()
    {
      Meta = new Sdx.Db.TableMeta()
      {
        Name = "shop_category",
        Pkeys = new List<string>()
        {
          "shop_id",
          "category_id"
        },
        Columns = new List<string>()
        {
          "shop_id",
          "category_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation(
              new Test.Orm.Table.Shop(),
              "shop_id",
              "id"
            )
          },
          {
            "category",
            new Relation(
              new Test.Orm.Table.Category(),
              "category_id",
              "id"
            )
          }
        }
      };
    }
  }
}
