using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Category : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Category()
    {
      Meta = new Sdx.Db.MetaData(
        "category",
        new List<string>()
        {
          "id"
        },
        new List<string>()
        {
          "id",
          "name",
          "code",
        },
        new Dictionary<string, Relation>()
        {
          {
            "shop_category",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "category_id"
            )
          }
        },
        typeof(Test.Orm.Category),
        typeof(Test.Orm.Table.Category)
      );
    }
  }
}
