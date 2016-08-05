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
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("code"))
        );


      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
          .Set("label", new Sdx.Scaffold.Config.Value("名称"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("code"))
          .Set("label", new Sdx.Scaffold.Config.Value("コード"))
          .Set("attributes", new Sdx.Scaffold.Config.Value("data-hoge", "foo"))
        );

      return scaffold;
    }
  }
}
