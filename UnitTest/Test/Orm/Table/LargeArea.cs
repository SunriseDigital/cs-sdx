﻿using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class LargeArea : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static LargeArea()
    {
      Meta = new Sdx.Db.TableMeta(
        name: "large_area",
        pkeys: new List<string>()
        {
          "id"
        },
        columns: new List<string>()
        {
          "id",
          "name",
          "code"
        },
        relations: new Dictionary<string, Relation>()
        {
          {
            "area",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "large_area_id"
            )
          }
        }
      );
    }
  }
}
