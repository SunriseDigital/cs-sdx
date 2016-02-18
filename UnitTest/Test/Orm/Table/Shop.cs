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
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true),
          new Column("name"),
          new Column("area_id"),
          new Column("main_image_id"),
          new Column("sub_image_id"),
          new Column("created_at"),
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

    public static Sdx.Html.FormElement CreateCategoryIdElement()
    {
      var elem = new Sdx.Html.CheckableGroup();
      elem.Name = "category_id";

      var db = Test.Db.Adapter.CreateDb();
      using(var conn = db.CreateConnection())
      {
        conn.Open();
        var select = db.CreateSelect();
        select.AddFrom(new Test.Orm.Table.Category()).Table.SelectDefaultOrder(select);

        conn.FetchRecordSet(select).ForEach(rec =>
        {
          var checkbox = new Sdx.Html.CheckBox();
          checkbox.Tag.Attr["value"] = rec.GetString("id");
          elem.AddCheckable(checkbox);
        });
      }

      return elem;
    }
  }
}
