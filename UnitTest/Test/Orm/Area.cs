using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class Area : Sdx.Db.Record
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Area()
    {
      Meta = Test.Orm.Table.Area.Meta;
    }
  }
}
