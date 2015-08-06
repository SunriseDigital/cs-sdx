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
        Columns = new List<string>()
        {
          "id",
          "name",
          "code",
          "category_type_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation()
            {
              ForeignKey = "id",
              ReferenceKey = "category_id"
            }
          }
        }
      };
    }
  }
}
