﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Test.Orm
{
  public class Area : Sdx.Db.Record
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Area()
    {
      Meta = Test.Orm.Table.Area.Meta;
    }

    public void SetNameWithCode(string nameWithCode)
    {
      var chunk = nameWithCode.Split(',');
      SetValue("name", chunk[0]);
      SetValue("code", chunk[1]);
    }

    public string GetNameWithCode()
    {
      return GetString("name") + "," + GetString("code");
    }



    public string[] Types
    {
      set
      {
        string values = string.Join("_",value);
        SetValue("name", values);
      }
    }

    public string TypesForString
    {
      set
      {
        SetValue("name", value);
      }
    }

  }
}
