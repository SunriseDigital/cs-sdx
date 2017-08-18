using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Orm
{
  public class ShopWithSchema: Shop
  {
    public static new Sdx.Db.TableMeta Meta
    {
      get
      {
        return Test.Orm.Table.ShopWithSchema.Meta;
      }
    }
  }
}
