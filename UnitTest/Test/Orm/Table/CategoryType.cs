using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class CategoryType : Sdx.Db.Table
  {
    override protected MetaData CreateTableMeta()
    {
      return new MetaData()
      {
        Name = "category_type",
        Columns = new List<string>()
        {
          "id",
          "name",
          "code",
        },
        Relations = new Dictionary<string, Relation>()
        {

        }
      };
    }
  }
}
