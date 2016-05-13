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

    public void SetTempPath(string[] values, NameValueCollection form)
    {
      Sdx.Context.Current.Debug.Log(values);
      Sdx.Context.Current.Debug.Log(form);
      this.SetValue("path", "", true);
    }

    public string[] GetImages()
    {
      return new string[1]{"/tmp/hogehoge.jpg"};
    }
  }
}
