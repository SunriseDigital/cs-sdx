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
              ForeignKey = "area_id",
              ReferenceKey = "id"
            }
          },
          {
            "main_image",
            new Relation()
            {
              TableName = "image",
              ForeignKey = "main_image_id",
              ReferenceKey = "id"
            }
          },
          {
            "sub_image",
            new Relation()
            {
              TableName = "image",
              ForeignKey = "sub_image_id",
              ReferenceKey = "id"
            }
          },
          {
            "menu",
            new Relation()
            {
              ForeignKey = "id",
              ReferenceKey = "shop_id"
            }
          }
        }
      };
    }
  }
}
