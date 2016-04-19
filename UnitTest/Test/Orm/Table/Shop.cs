using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Shop : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Shop()
    {
      Meta =  new Sdx.Db.TableMeta(
        "shop",
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true, isPkey: true),
          new Column("name"),
          new Column("area_id", type: ColumnType.Integer),
          new Column("main_image_id", isNotNull: false),
          new Column("sub_image_id", isNotNull: false),
          new Column("login_id", isNotNull: false),
          new Column("password", isNotNull: false),
          new Column("created_at", type: ColumnType.DateTime),
        },
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
      var elem = new Sdx.Html.CheckableGroup();
      elem.Name = "category_id";

      var select = conn.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Category()).Table.SelectDefaultOrder(select);
      select.ClearColumns().AddColumns("id", "name");

      conn.FetchKeyValuePairList<string, string>(select).ForEach(pair =>
      {
        elem.AddCheckable<Sdx.Html.CheckBox>(pair);
      });

      return elem;
    }

    public static Sdx.Html.FormElement CreateAreaIdElement(Sdx.Db.Connection conn)
    {
      var elem = new Sdx.Html.Select("area_id");

      var select = conn.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Area()).Table.SelectDefaultOrder(select);
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

    public static void CreateNameValidators(Sdx.Html.FormElement element)
    {
      Meta.GetColumn("name").AppendValidators(element);
      element.AddValidator(new Sdx.Validation.StringLength(min: 3, max: 50));
    }
  }
}
