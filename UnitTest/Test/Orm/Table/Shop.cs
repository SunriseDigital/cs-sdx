using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
      {
        Name = "shop",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "area_id",
          "main_image_id",
          "sub_image_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "area",
            new Relation(
              new Test.Orm.Table.Area(),
              "area_id",
              "id"
            )
          },
          {
            "main_image",
            new Relation(
              new Test.Orm.Table.Image(),
              "main_image_id",
              "id"
            )
          },
          {
            "sub_image",
            new Relation(
              new Test.Orm.Table.Image(),
              "sub_image_id",
              "id"
            )
          },
          {
            "menu",
            new Relation(
              new Test.Orm.Table.Menu(),
              "id",
              "shop_id"
            )
          },
          {
            "shop_category",
            new Relation(
              new Test.Orm.Table.ShopCategory(),
              "id",
              "shop_id"
            )
          }
        }
      };
    }
  }
}
