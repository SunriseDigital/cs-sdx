using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Area : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Area()
    {
      Meta = new Sdx.Db.TableMeta()
      {
        Name = "area",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "code",
          "large_area_id"
        },
        Relations = new Dictionary<string, Relation>()
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
      };
    }
  }
}
