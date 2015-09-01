using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Area : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Area()
    {
      Meta = new Sdx.Db.MetaData(
        "area",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id"),
          new Column("name"),
          new Column("code"),
          new Column("large_area_id"),
        },
        new Dictionary<string, Relation>()
        {
          {
            "large_area",
            new Relation(
              typeof(Test.Orm.Table.LargeArea),
              "large_area_id",
              "id"
            )
          }
        },
        typeof(Test.Orm.Area),
        typeof(Test.Orm.Table.Area)
      );
    }
  }
}
