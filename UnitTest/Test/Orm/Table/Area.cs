using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Area : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Area()
    {
      Meta = new Sdx.Db.TableMeta(
        name: "area",
        pkeys: new List<string>()
        {
          "id"
        },
        columns: new List<string>()
        {
          "id",
          "name",
          "code",
          "large_area_id"
        },
        relations: new Dictionary<string, Relation>()
        {
          {
            "large_area",
            new Relation(
              typeof(Test.Orm.Table.LargeArea),
              "large_area_id",
              "id"
            )
          }
        }
      );
    }
  }
}
