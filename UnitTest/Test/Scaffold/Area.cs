using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  public class Area
  {
    public static Sdx.Scaffold.Manager Create()
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, Test.Db.Adapter.CreateDb());
      scaffold.Title = "エリア";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

      //scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", "GetName", "GetList");
      scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name", "SelectDefaultOrder");

      scaffold.DisplayList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
        );

      scaffold.FormList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "id")
          .Set("label", "ID")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "large_area_id")
          .Set("label", "大エリア")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
          .Set("label", "名称")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
          .Set("label", "コード")
        );

      return scaffold;
    }
  }
}
