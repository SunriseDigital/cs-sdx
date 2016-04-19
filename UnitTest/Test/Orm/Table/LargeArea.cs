using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  class LargeArea : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    static LargeArea()
    {
      Meta = new Sdx.Db.TableMeta(
        "large_area",
        new List<Column>()
        {
          new Column("id", isAutoIncrement: true, isPkey: true),
          new Column("name"),
          new Column("code"),
        },
        new Dictionary<string, Relation>()
        {
          {
            "area",
            new Relation(
              typeof(Test.Orm.Table.ShopCategory),
              "id",
              "large_area_id"
            )
          }
        },
        typeof(Test.Orm.LargeArea),
        typeof(Test.Orm.Table.LargeArea)
      );
    }

    public static Sdx.Html.FormElement CreateIdElementForScaffold()
    {
      var elem = new Sdx.Html.TextArea();
      elem.Name = "id";
      elem.Tag.Attr["data-type"] = "scaffold";
      return elem;
    }

    public List<KeyValuePair<string, string>> FetchPairsForOption(Sdx.Db.Connection conn)
    {
      var select = conn.Adapter.CreateSelect();
      select.AddFrom(this);
      SelectDefaultOrder(select);

      select.ClearColumns().AddColumns("id", "name");

      return conn.FetchKeyValuePairList<string, string>(select);
    }
  }
}
