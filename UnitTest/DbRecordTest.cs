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
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestSimpleResult()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSimpleResult(db);
        ExecSql(db);
      }
    }

    private void RunSimpleResult(TestDb db)
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
        Assert.Equal("2", shop.GetString("area_id"));
        Assert.Equal("", shop.GetString("main_image_id"));
        Assert.Equal("", shop.GetString("sub_image_id"));
      });
    }

    [Fact]
    public void TestJoinOneMany()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunJoinOneMany(db);
        ExecSql(db);
      }
    }

    private void RunJoinOneMany(TestDb db)
    {
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Query.Order.ASC)
         .InnerJoin(new Test.Orm.Table.Area())
         ;
      select.Limit = 2;

      Sdx.Db.Query.Result result = select.Execute();
      var shops = result.Assemble("shop");
      Assert.Equal(2, shops.Count);

      Assert.Equal("1", shops[0].GetString("id"));
      Assert.Equal("天祥", shops[0].GetString("name"));
      Assert.Equal("2", shops[0].GetString("area_id"));
      Assert.Equal("", shops[0].GetString("main_image_id"));
      Assert.Equal("", shops[0].GetString("sub_image_id"));

      var area = shops[0].Assemble("area")[0];
      Assert.Equal("新中野", area.GetString("name"));

      //Assert.Equal("2", shops[1].GetString("id"));
      Assert.Equal("エスペリア", shops[1].GetString("name"));
      Assert.Equal("3", shops[1].GetString("area_id"));
      Assert.Equal("", shops[1].GetString("main_image_id"));
      Assert.Equal("", shops[1].GetString("sub_image_id"));

      area = shops[1].Assemble("area")[0];
      Assert.Equal("西麻布", area.GetString("name"));
    }
  }
}
