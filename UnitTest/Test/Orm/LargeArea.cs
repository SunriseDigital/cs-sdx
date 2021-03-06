﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class LargeArea : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static LargeArea()
    {
      Meta = Test.Orm.Table.LargeArea.Meta;
    }

    /// <summary>
    /// scaffoldでメソッドを動的に呼び出す機能のテスト用
    /// </summary>
    /// <returns></returns>
    public string GetCode()
    {
      return GetString("code");
    }
  }
}
