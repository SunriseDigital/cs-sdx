using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  public class Shop
  {
    public static Sdx.Scaffold.Manager Create()
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Shop.Meta, Sdx.Db.Adapter.Manager.Get("main").Write);
      scaffold.Title = "エリア";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/shop/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/shop/list.aspx");

      scaffold.DisplayList
        .Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("name"))
        ).Add(Sdx.Scaffold.ConfigItem.Create()
          .Set("column", new Sdx.Scaffold.ConfigValue("code"))
        );

      return scaffold;
    }
  }
}
