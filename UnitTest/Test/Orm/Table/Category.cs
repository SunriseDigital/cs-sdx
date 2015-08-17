using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Category : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
      {
        Name = "category",
        Pkeys = new List<string>()
        {
          "id"
        },
        Columns = new List<string>()
        {
          "id",
          "name",
          "code",
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "shop_category",
            new Relation(
              new Test.Orm.Table.ShopCategory(),
              "id",
              "category_id"
            )
          }
        }
      };
    }
  }
}
