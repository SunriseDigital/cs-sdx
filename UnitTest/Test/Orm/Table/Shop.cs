using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Shop()
    {
      Meta =  new Sdx.Db.MetaData(
        "shop",
        new List<string>()
        {
          "id"
        },
        new List<string>()
        {
          "id",
          "name",
          "area_id",
          "main_image_id",
          "sub_image_id"
        },
        new Dictionary<string, Relation>()
        {
          {
            "area",
            new Relation(
              typeof(Test.Orm.Table.Area),
              "area_id",
              "id"
            )
          },
          {
            "main_image",
            new Relation(
              typeof(Test.Orm.Table.Image),
              "main_image_id",
              "id"
            )
          },
          {
            "sub_image",
            new Relation(
              typeof(Test.Orm.Table.Image),
              "sub_image_id",
              "id"
            )
          },
          {
            "menu",
            new Relation(
              typeof(Test.Orm.Table.Menu),
              "id",
              "shop_id"
            )
          },
          {
            "shop_category",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "shop_id"
            )
          }
        },
        typeof(Test.Orm.Shop),
        typeof(Test.Orm.Table.Category)
      );
    }
  }
}
