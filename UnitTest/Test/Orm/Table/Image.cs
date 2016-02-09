using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Image : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Image()
    {
      Meta = new Sdx.Db.TableMeta(
        "image",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true),
          new Column("path"),
        },
        new Dictionary<string, Relation>()
        {

        },
        typeof(Test.Orm.Image),
        typeof(Test.Orm.Table.Image)
      );
    }
  }
}
