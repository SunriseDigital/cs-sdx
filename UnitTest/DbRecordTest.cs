using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;

namespace UnitTest
{
  [TestClass]
  public class DbRecordTest : BaseDbTest
  {
    [Fact]
    public void FetchSimpleRecord()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunCreateSelect(db);
        ExecSql(db);
      }
    }

    private void RunCreateSelect(TestDb db)
    {
      var tShop = new Test.Orm.Table.Shop();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(tShop)
         .AddOrder("id", Sdx.Db.Query.Order.ASC);
      select.Limit = 1;

      Sdx.Db.Query.Result result = select.Execute();
      var shops = result.Assemble("shop");
      shops.ForEach(shop => {
        Assert.Equal("1", shop.GetString("id"));
        Assert.Equal("天祥", shop.GetString("name"));
        Assert.Equal("1", shop.GetString("category_id"));
        Assert.Equal("", shop.GetString("main_image_id"));
        Assert.Equal("", shop.GetString("sub_image_id"));
      });

    }
  }
}
