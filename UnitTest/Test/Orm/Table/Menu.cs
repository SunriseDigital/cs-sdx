using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Menu : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Menu()
    {
      Meta = new Sdx.Db.TableMeta()
      {
        Name = "menu",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "shop_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation(
              typeof(Test.Orm.Table.Shop),
              "shop_id",
              "id"
            )
          }
        }
      };
    }
  }
}
