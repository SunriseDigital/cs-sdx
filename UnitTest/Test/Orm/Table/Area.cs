using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class Area : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static Area()
    {
      Meta = new Sdx.Db.TableMeta(
        "area",
        new List<string>()
        {
          "id"
        },
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true),
          new Column("name"),
          new Column("code"),
          new Column("large_area_id"),
        },
        new Dictionary<string, Relation>()
        {
          {
            "large_area",
            new Relation(
              typeof(Test.Orm.Table.LargeArea),
              "large_area_id",
              "id"
            )
          }
        },
        typeof(Test.Orm.Area),
        typeof(Test.Orm.Table.Area)
      );
    }

    public static Sdx.Html.FormElement CreateLargeAreaIdElement()
    {
      var elem = new Sdx.Html.Select();
      elem.Name = "large_area_id";

      var db = Test.Db.Adapter.CreateDb();
      using(var conn = db.CreateConnection())
      {
        conn.Open();
        var select = db.CreateSelect();
        select
          .AddFrom(new Test.Orm.Table.LargeArea())
          .ClearColumns()
          .AddColumns("id", "name").Table.SelectDefaultOrder(select);

        elem.AddOption(Sdx.Html.Option.Create("", "大エリアを選択"));

        conn.FetchKeyValuePairList<string, string>(select).ForEach((pair) => {
          elem.AddOption(Sdx.Html.Option.Create(pair));
        });
      }

      return elem;
    }
  }
}
