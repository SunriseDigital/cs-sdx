﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class ShopCategory : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopCategory()
    {
      Meta = Test.Orm.Table.ShopCategory.Meta;
    }
  }
}
