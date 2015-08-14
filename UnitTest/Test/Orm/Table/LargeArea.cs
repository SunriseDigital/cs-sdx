using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class LargeArea : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
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
            new Relation()
            {
              ForeignKey = "id",
              ReferenceKey = "large_area_id"
            }
          }
        }
      };
    }
  }
}
