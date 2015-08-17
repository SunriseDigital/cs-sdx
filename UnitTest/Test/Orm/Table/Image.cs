using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Image : Sdx.Db.Table
  {
    public static new Sdx.Db.TableMeta Meta { get; private set; }

    static Image()
    {
      Meta = new Sdx.Db.TableMeta()
      {
        Name = "image",
        Columns = new List<string>()
        {
          "id",
          "path"
        },
        Relations = new Dictionary<string, Relation>()
        {

        }
      };
    }
  }
}
