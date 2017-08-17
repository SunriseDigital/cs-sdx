using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Orm.Table
{
  public class MenuWithSchema : Menu
  {
    public static new Sdx.Db.TableMeta Meta { get; private set; }

    static MenuWithSchema()
    {
      var Relations = CreateRelations();

      Meta = new Sdx.Db.TableMeta(
        "dbo.shop",
        CreateColumns(),
        Relations,
        typeof(Test.Orm.MenuWithSchema),
        typeof(Test.Orm.Table.MenuWithSchema)
      );
    }
  }
}
