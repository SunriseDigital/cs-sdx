using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class ShopCategory : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopCategory()
    {
      Meta = new Sdx.Db.TableMeta(
        "shop_category",
        new List<Column>()
        {
          new Column("shop_id", isPkey: true),
          new Column("category_id", isPkey: true),
        },
        new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation(
              typeof(Test.Orm.Table.Shop),
              "shop_id",
              "id"
            )
          },
          {
            "category",
            new Relation(
              typeof(Test.Orm.Table.Category),
              "category_id",
              "id"
            )
          }
        },
        typeof(Test.Orm.ShopCategory),
        typeof(Test.Orm.Table.ShopCategory)
      );
    }
  }
}
