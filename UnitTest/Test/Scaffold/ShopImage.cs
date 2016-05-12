using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
  public class ShopImage
  {
    public static Sdx.Scaffold.Manager Create(Sdx.Db.Adapter.Base db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.ShopImage.Meta, Sdx.Db.Adapter.Manager.Get("main").Write);
      scaffold.Title = "店舗画像";

      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/shop-image/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/shop-image/list.aspx");

      scaffold.Group = new Sdx.Scaffold.Group.TableMeta(
        "shop_id",
        Test.Orm.Table.Shop.Meta,
        new Sdx.Scaffold.Config.Value("#GetNameWithArea")
      );

      scaffold.DisplayList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("タイプ"))
          .Set("dynamic", new Sdx.Scaffold.Config.Value("@shop_image_type.name"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("画像"))
          .Set("html", new Sdx.Scaffold.Config.Value(@"<img src=""{#GetImageWebPath}"" />"))
        )
        ;

      scaffold.ListSelectHook = new Sdx.Scaffold.Config.Value("SelectDefaultOrder");


      //保存用のフォーム項目設定
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("shop_image_type_id"))
          .Set("label", new Sdx.Scaffold.Config.Value("タイプ"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          //.Set("column", new Sdx.Scaffold.Config.Value("path"))
          .Set("label", new Sdx.Scaffold.Config.Value("画像"))
          .Set("setter", new Sdx.Scaffold.Config.Value("SetTempPath"))
          .Set("getter", new Sdx.Scaffold.Config.Value("GetImages"))
          .Set("name", new Sdx.Scaffold.Config.Value("path"))
          .Set("multiple", new Sdx.Scaffold.Config.Value("on"))
        )
        
        ;

      return scaffold;
    }
  }
}
