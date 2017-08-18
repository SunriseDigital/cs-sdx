using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Orm
{
  public class MenuWithSchema : Menu
  {
    public static new Sdx.Db.TableMeta Meta
    {
      get
      {
        return Test.Orm.Table.MenuWithSchema.Meta;
      }
    }
  }
}
