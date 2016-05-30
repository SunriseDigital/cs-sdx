using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class ShopImageType : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopImageType()
    {
      Meta = new Sdx.Db.TableMeta(
        "shop_image_type",
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true, isPkey: true),
          new Column("name")
        },
        new Dictionary<string, Relation>()
        {
          {
            "shop_image",
            new Relation(
              typeof(Test.Orm.Table.ShopImage),
              "id",
              "shop_image_type_id"
            )
          }
        },
        typeof(Test.Orm.ShopImageType),
        typeof(Test.Orm.Table.ShopImageType)
      );
    }
  }
}
