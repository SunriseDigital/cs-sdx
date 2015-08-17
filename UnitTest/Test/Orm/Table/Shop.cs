﻿using System;
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
            new Relation()
            {
              Table = new Test.Orm.Table.Area(),
              ForeignKey = "area_id",
              ReferenceKey = "id"
            }
          },
          {
            "main_image",
            new Relation()
            {
              Table = new Test.Orm.Table.Image(),
              ForeignKey = "main_image_id",
              ReferenceKey = "id"
            }
          },
          {
            "sub_image",
            new Relation()
            {
              Table = new Test.Orm.Table.Image(),
              ForeignKey = "sub_image_id",
              ReferenceKey = "id"
            }
          },
          {
            "menu",
            new Relation()
            {
              Table = new Test.Orm.Table.Menu(),
              ForeignKey = "id",
              ReferenceKey = "shop_id"
            }
          },
          {
            "shop_category",
            new Relation()
            {
              Table = new Test.Orm.Table.ShopCategory(),
              ForeignKey = "id",
              ReferenceKey = "shop_id"
            }
          }
        }
      };
    }
  }
}
