using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Category : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Category()
    {
      Meta = new Sdx.Db.TableMeta(
        name: "category",
        pkeys: new List<string>()
        {
          "id"
        },
        columns: new List<string>()
        {
          "id",
          "name",
          "code",
        },
        relations: new Dictionary<string, Relation>()
        {
          {
            "shop_category",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "category_id"
            )
          }
        }
      );
    }
  }
}
