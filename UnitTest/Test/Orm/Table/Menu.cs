using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Menu : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Menu()
    {
      Meta = new Sdx.Db.TableMeta(
        "menu",
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true, isPkey: true),
          new Column("name"),
          new Column("shop_id"),
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
