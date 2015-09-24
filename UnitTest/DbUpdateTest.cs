using Xunit;
using UnitTest.DummyClasses;
using System.Collections.Generic;
using System.Data.Common;

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
  public class DbUpdateTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestUpdate()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunUpdate(db);
      }
    }

    private void RunUpdate(TestDb testDb)
    {
      //var db = new Sdx.Db.SqlServerAdapter();
      //db.ConnectionString = "SomeConnectionString...";

      //using (var conn = db.CreateConnection())
      //{
      //  conn.Open();
      //  using (var transaction = conn.BeginTransaction())
      //  {
      //    db.Insert("shop", new Dictionary<string, string> {
      //      { "name", "FooBar" },
      //      { "area_id", "1"}
      //    });
      //  }
      //}

      //var db = testDb.Adapter;

      //var shop = new Test.Orm.Shop();
      //shop
      //  .Set("name", "FooBar")
      //  .Set("area_id", "1");

      //using (var conn = db.CreateConnection())
      //{
      //  conn.Open();
      //  using (var transaction = conn.BeginTransaction())
      //  {
      //    shop.Save(transaction);
      //  }
      //}
    }
  }
}
