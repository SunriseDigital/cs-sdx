using System;
using System.Collections.Generic;
using System.Web;

namespace Test.Orm.Table
{
  class ShopImage : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static ShopImage()
    {
      Meta = new Sdx.Db.TableMeta(
        "shop_image",
        new List<Column>()
        {
          new Column("shop_id", isPkey: true),
          new Column("shop_image_type_id", isPkey: true),
          new Column("path"),
          new Column("created_at")
        },
        new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation(
              typeof(Test.Orm.Table.Shop),
              "shop_id",
              "id"
            )
          },
          {
            "shop_image_type",
            new Relation(
              typeof(Test.Orm.Table.ShopImageType),
              "shop_image_type_id",
              "id"
            )
          }
        },
        typeof(Test.Orm.ShopImage),
        typeof(Test.Orm.Table.ShopImage)
      );
    }

    public static Sdx.Html.FormElement CreateShopImageTypeIdElement(Sdx.Db.Connection conn)
    {
      var elem = new Sdx.Html.Select("shop_image_type_id");

      var select = conn.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.ShopImageType()).Table.SelectDefaultOrder(select, conn);
      select.ClearColumns().AddColumns("id", "name");

      elem.AddOption("", "タイプを選択してください");
      conn.FetchKeyValuePairList<string, string>(select).ForEach(pair =>
      {
        elem.AddOption(pair);
      });

      return elem;
    }

    public static Sdx.Html.FormElement CreatePathElement(Sdx.Db.Connection conn)
    {
      var elem = new Sdx.Html.ImageUploader("path");

      elem.ButtonLabel = new Sdx.Html.RawText("画像をアップロード");
      elem.UploadPath = "/form/upload-point.aspx";
      elem.ThumbWidth = 200;
      elem.ThumbHeight = 100;
      elem.DeleteLabel = @"<i class=""fa fa-times"" aria-hidden=""true""></i>";

      return elem;
    }
  }
}
