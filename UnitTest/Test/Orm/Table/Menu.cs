using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Menu : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
      {
        Name = "menu",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "shop_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation()
            {
              ForeignKey = "shop_id",
              ReferenceKey = "id"
            }
          }
        }
      };
    }
  }
}
