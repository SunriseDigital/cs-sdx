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
          new Column("sequence"),
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

    public static Sdx.Html.FormElement CreateLargeAreaIdElement(Sdx.Db.Connection conn)
    {
      var elem = new Sdx.Html.Select();
      elem.Name = "large_area_id";

      var select = conn.Adapter.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.LargeArea())
        .ClearColumns()
        .AddColumns("id", "name").Table.SelectDefaultOrder(select);

      elem.AddOption("", "大エリアを選択");

      select.FetchKeyValuePairList<string, string>(conn).ForEach((pair) =>
      {
        elem.AddOption(pair);
      });

      return elem;
    }
  }
}
