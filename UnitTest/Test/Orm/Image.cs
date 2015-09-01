using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class Image : Sdx.Db.Record
  {
    public static Sdx.Db.MetaData Meta { get; private set; }

    static Image()
    {
      Meta = Test.Orm.Table.Image.Meta;
    }
  }
}
      