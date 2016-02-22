using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  public class LargeArea
  {
    public static Sdx.Scaffold.Manager Create()
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, Sdx.Db.Adapter.Manager.Get("main").Write);
      scaffold.Title = "大エリア";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/large-area/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/large-area/list.aspx");

      scaffold.DisplayList
        .Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("name"))
        ).Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("code"))
        );


      scaffold.FormList
        .Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("id"))
          .Set("label", new Sdx.Scaffold.ConfigValue("ID"))
        ).Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("name"))
          .Set("label", new Sdx.Scaffold.ConfigValue("名称"))
        ).Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("code"))
          .Set("label", new Sdx.Scaffold.ConfigValue("コード"))
        );


      return scaffold;
    }
  }
}
