using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class ShopCategory : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopCategory()
    {
      Meta = new Sdx.Db.TableMeta(
        name: "shop_category",
        pkeys: new List<string>()
        {
          "shop_id",
          "category_id"
        },
        columns: new List<string>()
        {
          "shop_id",
          "category_id"
        },
        relations: new Dictionary<string, Relation>()
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
        }
      );
    }
  }
}
