using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Image : Sdx.Db.Table
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Image()
    {
      Meta = new Sdx.Db.MetaData(
        "image",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id"),
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
