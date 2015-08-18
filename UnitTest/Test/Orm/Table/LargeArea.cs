using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class LargeArea : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static LargeArea()
    {
      Meta = new Sdx.Db.MetaData(
        "large_area",
        new List<string>()
        {
          "id"
        },
        new List<string>()
        {
          "id",
          "name",
          "code"
        },
        new Dictionary<string, Relation>()
        {
          {
            "area",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "large_area_id"
            )
          }
        },
        typeof(Test.Orm.LargeArea),
        typeof(Test.Orm.Table.LargeArea)
      );
    }
  }
}
