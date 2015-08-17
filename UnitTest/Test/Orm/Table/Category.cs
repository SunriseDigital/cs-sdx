using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Category : Sdx.Db.Table
  {
    public static new Sdx.Db.TableMeta Meta { get; private set; }

    static Category()
    {
      Meta = new Sdx.Db.TableMeta()
      {
        Name = "category",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "code",
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "shop_category",
            new Relation(
              new Test.Orm.Table.ShopCategory(),
              "id",
              "category_id"
            )
          }
        }
      };
    }
  }
}
