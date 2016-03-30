using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  using Config = Sdx.Scaffold.Config;
  public class Shop
  {
    public static Sdx.Scaffold.Manager Create(Sdx.Db.Adapter.Base db, string scaffoldName = null)
    {
      Sdx.Scaffold.Manager scaffold;
      if(scaffoldName == null)
      {
        scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Shop.Meta, db);
      }
      else
      {
        scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Shop.Meta, db, scaffoldName);
      }
      scaffold.Title = "店舗";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/shop/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/shop/list.aspx");

      scaffold.DisplayList
        .Add(Config.Item.Create()
          .Set("label", new Config.Value("名前"))
          .Set("column", new Config.Value("name"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Config.Value("エリア"))
          .Set("dynamic", new Config.Value("@area.name"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Config.Value("ログインID"))
          .Set("column", new Config.Value("login_id"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Config.Value("業種"))
          .Set("dynamic", new Config.Value("#GetCategoryNames"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Config.Value("登録日時"))
          .Set("column", new Config.Value("created_at"))
        );

      scaffold.PerPage = 2;
      scaffold.ListSelectHook = new Config.Value("SelectDefaultOrder");

      scaffold.FormList
        .Add(Config.Item.Create()
          .Set("column", new Config.Value("name"))
          .Set("label", new Config.Value("店名"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("area_id"))
          .Set("label", new Config.Value("場所"))
        ).Add(Config.Item.Create()
          .Set("label", new Config.Value("業種"))
          .Set("relation", new Config.Value("shop_category"))
          .Set("column", new Config.Value("category_id"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("login_id"))
          .Set("label", new Config.Value("ログインID"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("password"))
          .Set("label", new Config.Value("パスワード"))
          .Set("setter", new Config.Value("SetRawPassword"))
        ).Add(Config.Item.Create()
          .Set("column", new Config.Value("created_at"))
          .Set("label", new Config.Value("登録日時"))
        );

      return scaffold;
    }
  }
}
