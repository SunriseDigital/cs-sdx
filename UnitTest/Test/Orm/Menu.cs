using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class Menu : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Menu()
    {
      Meta = Test.Orm.Table.Menu.Meta;
    }
  }
}
