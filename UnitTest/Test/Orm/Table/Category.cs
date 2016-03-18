using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Category : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Category()
    {
      Meta = new Sdx.Db.TableMeta(
        "category",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true),
          new Column("name"),
          new Column("code"),
        },
        new Dictionary<string, Relation>()
        {
          {
            "shop",
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
