using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Menu : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Menu()
    {
      Meta = new Sdx.Db.MetaData(
        "menu",
        new List<string>()
        {
          "id"
        },
        new List<string>()
        {
          "id",
          "name",
          "shop_id"
        },
        new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation(
              typeof(Test.Orm.Table.Shop),
              "shop_id",
              "id"
            )
          }
        },
        typeof(Test.Orm.Menu),
        typeof(Test.Orm.Table.Menu)
      );
    }
  }
}
