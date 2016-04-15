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
  public class Db_Sql_Delete : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestDelete()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunDelete(db);
      }
    }

    private void RunDelete(TestDb testDb)
    {
      var db = testDb.Adapter;

      //まずは入れてみる
      var insert = db.CreateInsert();
      insert
        .AddColumnValue("name", "Delete")
        .AddColumnValue("area_id", 1)
        .AddColumnValue("created_at", DateTime.Now)
        .SetInto("shop")
         ;

      object id = 0;
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        conn.Execute(insert);
        id = conn.FetchLastInsertId();
        conn.Commit();
      }

      //確認
      var select = db.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", id);

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = select.FetchRecord(conn);

        Assert.Equal("Delete", shop.GetValue("name"));
      }

      //delete
      var delete = db.CreateDelete();

      delete
        .SetFrom("shop")
        .Where.Add("id", id);

      using (var command = delete.Build())
      {
        Assert.Equal(testDb.Sql(@"DELETE FROM {0}shop{1} WHERE {0}id{1} = @0"), command.CommandText);
        Assert.Equal(id, command.Parameters["@0"].Value);

        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();
          var count = conn.ExecuteNonQuery(command);
          Assert.Equal(1, count);
          conn.Commit();
        }
      }

      //確認
      select = db.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", id);

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = select.FetchRecord(conn);

        Assert.Null(shop);
      }
    }

    [Fact]
    public void TestRecordSaveAndDelete()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunRecordSaveAndDelete(db);
      }
    }

    private void RunRecordSaveAndDelete(TestDb testDb)
    {
      var db = testDb.Adapter;

      var shop = new Test.Orm.Shop();
      Assert.False(shop.IsUpdated);
      Assert.True(shop.IsNew);

      var newName = Sdx.Util.String.GenRandom(10);
      var dateTime = Sdx.Util.Datetime.RoundTicks(DateTime.Now);
      shop.SetValue("name", newName);
      Assert.True(shop.IsUpdated);
      shop.SetValue("area_id", 3);
      shop.SetValue("main_image_id", 2);
      shop.SetValue("created_at", dateTime);

      Assert.Equal(newName, shop.GetString("name"));
      Assert.Equal(3, shop.GetInt32("area_id"));
      Assert.Equal(2, shop.GetInt32("main_image_id"));
      Assert.Equal(dateTime, shop.GetDateTime("created_at"));
      Assert.True(shop.IsNew);

      using (var conn = db.CreateConnection())
      {
        //新規保存
        conn.Open();
        conn.BeginTransaction();
        shop.Save(conn);
        conn.Commit();

        Assert.False(shop.IsUpdated);
        Assert.False(shop.IsNew);

        var id = shop.GetValue("id");
        Assert.NotNull(id);

        var select = db.CreateSelect();
        select
            .AddFrom(new Test.Orm.Table.Shop())
            .Where.Add("id", id);

        var newShop = select.FetchRecord(conn);

        Assert.Equal(newName, newShop.GetString("name"));
        Assert.Equal(3, newShop.GetInt32("area_id"));
        Assert.Equal(2, shop.GetInt32("main_image_id"));
        Assert.Equal(dateTime, newShop.GetDateTime("created_at"));
        Assert.False(newShop.IsDeleted);

        //削除
        conn.BeginTransaction();
        newShop.Delete(conn);
        conn.Commit();

        Assert.True(newShop.IsDeleted);

        //確認
        var deletedShop = select.FetchRecord(conn);
        Assert.Null(deletedShop);
      }
    }
  }
}
