using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Image : Sdx.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Image()
    {
      Meta = new Sdx.Db.TableMeta(
        name: "image",
        pkeys: new List<string>(),
        columns: new List<string>()
        {
          "id",
          "path"
        },
        relations: new Dictionary<string, Relation>()
        {

        }
      );
    }
  }
}
