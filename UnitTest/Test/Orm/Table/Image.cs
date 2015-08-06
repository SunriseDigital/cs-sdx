using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Image : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
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
