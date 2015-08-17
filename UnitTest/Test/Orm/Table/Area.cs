﻿using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Area : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
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
            new Relation()
            {
              Table = new Test.Orm.Table.LargeArea(),
              ForeignKey = "large_area_id",
              ReferenceKey = "id"
            }
          }
        }
      };
    }
  }
}
