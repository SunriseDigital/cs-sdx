using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class LargeArea : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static LargeArea()
    {
      Meta = new Sdx.Db.TableMeta()
      {
        Name = "large_area",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "code"
        },
        Relations = new Dictionary<string, Relation>()
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
      };
    }
  }
}
