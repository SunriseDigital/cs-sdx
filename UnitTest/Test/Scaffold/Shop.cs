using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Scaffold
{
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

      //ページの行き来をするのにお互いのURLが必要です。
      scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/shop/edit.aspx");
      scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/shop/list.aspx");

      //リストに表示する項目の設定
      scaffold.DisplayList
          //各項目はSdx.Scaffold.Config.Itemのインスタンスで追加します。
          .Add(Sdx.Scaffold.Config.Item.Create()
            //一つの項目に幾つかの設定値があり、Sdx.Scaffold.Config.Valueのインスタンスでセットします。
            .Set("label", new Sdx.Scaffold.Config.Value("ID"))
            .Set("column", new Sdx.Scaffold.Config.Value("id"))
            //項目の幅を指定します。レスポンシブを想定しているため特殊なコーディングをしています。
            //幅を指定しないと縦の列がきれいに揃いませんので注意してください。
            .Set("style", new Sdx.Scaffold.Config.Value("width: 70px;"))
          )
          .Add(Sdx.Scaffold.Config.Item.Create()
            .Set("label", new Sdx.Scaffold.Config.Value("名前"))
            .Set("column", new Sdx.Scaffold.Config.Value("name"))
          )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("エリア"))
          //`@area.name` = `shop.GetRecord('area').GetString("name")`
          .Set("dynamic", new Sdx.Scaffold.Config.Value("@area.name"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("業種"))
          //`#GetCategoryNames` = `shop.GetCategoryNames(conn)`
          //Shopのレコードに`public string GetCategoryNames(Sdx.Db.Connection conn = null)`メソッドが実装されています。
          .Set("dynamic", new Sdx.Scaffold.Config.Value("#GetCategoryNames"))
          //classは追加でclass属性を付与します。柔軟にスタイルを利かせたい場合に使用します。
          .Set("class", new Sdx.Scaffold.Config.Value("category"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("画像"))
          .Set("html", new Sdx.Scaffold.Config.Value("<a href=\"/scaffold/shop-image/list.aspx?shop_id={id}\">{@area.#GetNameWithCode}</a>"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 100px;"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("ログインID"))
          .Set("column", new Sdx.Scaffold.Config.Value("login_id"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("登録日時"))
          .Set("column", new Sdx.Scaffold.Config.Value("created_at"))
        );

      //一ページ１０件のページネーションが付与される。省略すると全件表示されます。
      scaffold.PerPage = 10;

      //リストの並び順は`ListSelectHook`で指定。
      scaffold.ListSelectHook = new Sdx.Scaffold.Config.Value("SelectDefaultOrder");

      //下記のように文字列で指定するとTest.Orm.Table.Shop.SelectDefaultOrderが呼ばれます。
      //scaffold.ListSelectHook = new Sdx.Scaffold.Config.Value("SelectDefaultOrder");

      //保存用のフォーム項目設定
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
          .Set("label", new Sdx.Scaffold.Config.Value("店名"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("area_id"))
          .Set("label", new Sdx.Scaffold.Config.Value("場所"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("業種"))
          .Set("relation", new Sdx.Scaffold.Config.Value("shop_category"))
          .Set("column", new Sdx.Scaffold.Config.Value("category_id"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("login_id"))
          .Set("label", new Sdx.Scaffold.Config.Value("ログインID"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("password"))
          .Set("label", new Sdx.Scaffold.Config.Value("パスワード"))
          .Set("setter", new Sdx.Scaffold.Config.Value("SetRawPassword"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("created_at"))
          .Set("label", new Sdx.Scaffold.Config.Value("登録日時"))
          //autoCurrentCheckboxは「現在日時で更新」のチェックボックスが付与され
          //チェックを入れて送信すると現在日時で更新されます。
          .Set("autoCurrentCheckbox", new Sdx.Scaffold.Config.Value("auto_created_at"))
        );

      return scaffold;
    }
  }
}
