using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class Shop : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Shop()
    {
      Meta = Test.Orm.Table.Shop.Meta;
    }

    public void SetRawPassword(string rawPassword)
    {
      //本当はhash化するが確認しやすいように文字列追加のみ。
      SetValue("password", "HASH@" + rawPassword);
    }
  }
}
      