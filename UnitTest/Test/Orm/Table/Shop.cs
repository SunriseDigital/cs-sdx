using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  public class Shop : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Shop()
    {
      var db = Sdx.Db.Adapter.Manager.Get("mysql").Read;
      Meta =  new Sdx.Db.TableMeta(
        "shop",
        db.FetchColumns("shop"),
        new Dictionary<string, Relation>()
        {
          {
            "area",
            new Relation(
              typeof(Test.Orm.Table.Area),
              "area_id",
              "id"
            )
          },
          {
            "main_image",
            new Relation(
              typeof(Test.Orm.Table.Image),
              "main_image_id",
              "id"
            )
          },
          {
            "sub_image",
            new Relation(
              typeof(Test.Orm.Table.Image),
              "sub_image_id",
              "id"
            )
          },
          {
            "menu",
            new Relation(
              typeof(Test.Orm.Table.Menu),
              "id",
              "shop_id"
            )
          },
          {
            "shop_category",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "shop_id"
            )
          }
        },
        typeof(Test.Orm.Shop),
        typeof(Test.Orm.Table.Shop)
      );
    }

    public static Sdx.Html.FormElement CreateCategoryIdElement(Sdx.Db.Connection conn)
    {
      var elem = new Sdx.Html.CheckBoxGroup();
      elem.Name = "category_id";

      var select = conn.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Category()).Table.SelectDefaultOrder(select, conn);
      select.ClearColumns().AddColumns("id", "name");

      conn.FetchKeyValuePairList<string, string>(select).ForEach(pair =>
      {
        elem.AddCheckable(pair);
      });

      return elem;
    }

    public static Sdx.Html.FormElement CreateAreaIdElement(Sdx.Db.Connection conn)
    {
      var elem = new Sdx.Html.Select("area_id");

      var select = conn.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Area()).Table.SelectDefaultOrder(select, conn);
      select.ClearColumns().AddColumns("id", "name");

      elem.AddOption("", "場所を選択してください");
      conn.FetchKeyValuePairList<string, string>(select).ForEach(pair =>
      {
        elem.AddOption(pair);
      });

      return elem;
    }

    public static Sdx.Html.FormElement CreatePasswordElement()
    {
      var elem = new Sdx.Html.InputText("password");
      elem.IsSecret = true;

      return elem;
    }

    //public static void CreateNameValidators(Sdx.Html.FormElement element, Sdx.Db.Record record, Sdx.Db.Connection conn)
    //{
    //  Meta.GetColumn("name").AppendValidators(element, record);
    //}
  }
}
