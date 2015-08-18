using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class ShopCategory : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static ShopCategory()
    {
      Meta = new Sdx.Db.MetaData(
        "shop_category",
        new List<string>()
        {
          "shop_id",
          "category_id"
        },
        new List<Column>()
        {
          new Column("shop_id"),
          new Column("category_id"),
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
