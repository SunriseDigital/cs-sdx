using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Sdx.Db.Table
  {
    override protected Sdx.Db.TableMeta CreateMeta()
    {
      return new Sdx.Db.TableMeta()
      {
        Name = "shop",
        Columns = new List<string>()
        {
          "id",
          "name",
          "category_id",
          "main_image_id",
          "sub_image_id"
        }
      };
    }
  }
}
