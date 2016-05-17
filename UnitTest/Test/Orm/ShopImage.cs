using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class ShopImage : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopImage()
    {
      Meta = Test.Orm.Table.ShopImage.Meta;
    }

    public void SetTempPath(string value)
    {
      this.SetValue("path", value + ".hoge");
    }

    public string GetImageWebPath()
    {
      var path = GetString("path");
      return path.Substring(0, path.Length - 5);
    }
  }
}
