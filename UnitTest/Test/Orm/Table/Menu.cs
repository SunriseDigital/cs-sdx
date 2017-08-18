using System;
using System.Collections.Generic;

namespace Test.Orm.Table
{
  public class Menu : Test.Db.Table
  {
    public static Sdx.Db.TableMeta Meta { get; private set; }

    protected static List<Column> CreateColumns()
    {
      return new List<Column>()
        {
          new Column("id", isAutoIncrement: true, isPkey: true),
          new Column("name"),
          new Column("shop_id"),
        };
    }

    protected static Dictionary<string, Relation> CreateRelations()
    {
      return new Dictionary<string, Relation>()
        {
          {
            "shop",
            new Relation(
              typeof(Test.Orm.Table.Shop),
              "shop_id",
              "id"
            )
          }
        };
    }

    static Menu()
    {
      Meta = new Sdx.Db.TableMeta(
        "menu",
        CreateColumns(),
        CreateRelations(),
        typeof(Test.Orm.Menu),
        typeof(Test.Orm.Table.Menu)
      );
    }
  }
}
