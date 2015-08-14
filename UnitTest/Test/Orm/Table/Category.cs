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
          "category_type_id"
        },
        Relations = new Dictionary<string, Relation>()
        {
          {
            "category_type",
            new Relation()
            {
              ForeignKey = "category_type_id",
              ReferenceKey = "id"
            }
          }
        }
      };
    }
  }
}
