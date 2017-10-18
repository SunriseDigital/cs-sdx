using Xunit;
using UnitTest.DummyClasses;
using System;
using System.Collections.Generic;
using System.Data.Common;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

namespace UnitTest
{
  [TestClass]
  public class Db_Sql_Context : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestGetJoinedContext()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGetJoinedContext(db);
      }
    }

    private void RunGetJoinedContext(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop(), "shop1", cShop1 =>
      {
        cShop1.InnerJoin(new Test.Orm.Table.ShopCategory(), cShopCategory =>
        {
          cShopCategory.InnerJoin(new Test.Orm.Table.Shop(), "shop2");
        });

        cShop1.InnerJoin(new Test.Orm.Table.Menu(), cShopMenu =>
        {
          cShopMenu.InnerJoin(new Test.Orm.Table.Shop(), "shop3");
        });
      });

      var cCategory = select.Context("shop_category");
      var cMenu = select.Context("menu");

      Assert.Equal("shop2", cCategory.GetJoinedContexts<Test.Orm.Table.Shop>().First.Name);
      Assert.Equal("shop3", cMenu.GetJoinedContexts<Test.Orm.Table.Shop>().First.Name);

    }
  }
}
