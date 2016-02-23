using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  using Config = Sdx.Scaffold.Config;
  public class Area
  {
    public static Sdx.Scaffold.Manager Create()
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, Sdx.Db.Adapter.Manager.Get("main").Write);
      scaffold.Title = "エリア";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

      //scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), "GetName");
      scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name", "SelectDefaultOrder");

      scaffold.DisplayList
        .Add(Config.Item.Create()
          .Set("column", new Config.Value("name"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("code"))
        );

      scaffold.FormList
        .Add(Config.Item.Create()
          .Set("column", new Config.Value("id"))
          .Set("label", new Config.Value("ID"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("large_area_id"))
          .Set("label", new Config.Value("大エリア"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("name"))
          .Set("label", new Config.Value("名称"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("code"))
          .Set("label", new Config.Value("コード"))
        );

      return scaffold;
    }
  }
}
