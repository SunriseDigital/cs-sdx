using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
      {
        Name = "shop",
        Columns = new List<string>()
        {
          "id",
          "name",
          "category_id",
          "main_image_id",
          "sub_image_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "category",
            new Relation()
            {
              ForeignKey = "category_id",
              ReferenceKey = "id"
            }
          }
        }
      };
    }
  }
}
