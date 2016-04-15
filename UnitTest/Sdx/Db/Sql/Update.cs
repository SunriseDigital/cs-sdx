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
  public class Db_Sql_Update : BaseDbTest
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
      var db = testDb.Adapter;

      var update = db.CreateUpdate();

      update
        .SetTable("shop")
        .AddColumnValue("name", "UpdateTest")
        .AddColumnValue("area_id", 3)
        .Where.Add("id", 2);

      using (var command = update.Build())
      {
        Assert.Equal(testDb.Sql(@"UPDATE
          {0}shop{1}
          SET
            {0}name{1} = @0,
            {0}area_id{1} = @1
          WHERE {0}id{1} = @2"), command.CommandText);

        Assert.Equal("UpdateTest", command.Parameters["@0"].Value);
        Assert.Equal(3, command.Parameters["@1"].Value);
        Assert.Equal(2, command.Parameters["@2"].Value);

        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();
          conn.ExecuteNonQuery(command);
          conn.Commit();
        }
      }

      //確認
      var select = db.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", 2);

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord(select);

        Assert.Equal("UpdateTest", shop.GetValue("name"));
        Assert.Equal(3, shop.GetValue("area_id"));
      }
    }

    [Fact]
    public void TestUpdateSubquery()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunUpdateSubquery(db);
      }
    }

    private void RunUpdateSubquery(TestDb testDb)
    {
      var db = testDb.Adapter;

      var update = db.CreateUpdate();

      //Literal
      update
        .SetTable("shop")
        .AddColumnValue("area_id", Sdx.Db.Sql.Expr.Wrap("(SELECT id FROM area WHERE id = 5)"))
        .Where.Add("id", 3);

      using (var command = update.Build())
      {
        Assert.Equal(testDb.Sql(@"UPDATE {0}shop{1}
          SET
            {0}area_id{1} = (SELECT id FROM area WHERE id = 5) 
          WHERE {0}id{1} = @0"), command.CommandText);

        Assert.Equal(3, command.Parameters["@0"].Value);

        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();
          conn.ExecuteNonQuery(command);
          conn.Commit();
        }
      }

      //確認
      var select = db.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", 3);

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord(select);

        Assert.Equal(5, shop.GetValue("area_id"));
      }

      //Select
      update = db.CreateUpdate();

      select = db.CreateSelect();

      select
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", 4);

      //Literal
      update
        .SetTable("shop")
        .AddColumnValue("area_id", select)
        .Where.Add("id", 3);

      using (var command = update.Build())
      {
        Assert.Equal(testDb.Sql(@"UPDATE {0}shop{1}
          SET
            {0}area_id{1} = (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}id{1} = @0)
            WHERE {0}id{1} = @1"), command.CommandText);

        Assert.Equal(4, command.Parameters["@0"].Value);
        Assert.Equal(3, command.Parameters["@1"].Value);

        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();
          conn.ExecuteNonQuery(command);
          conn.Commit();
        }
      }

      //確認
      select = db.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", 3);

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord(select);

        Assert.Equal(4, shop.GetValue("area_id"));
      }
    }

    [Fact]
    public void TestRecordUpdate()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunRecordUpdate(db);
      }
    }

    private void RunRecordUpdate(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select
          .AddFrom(new Test.Orm.Table.Shop())
          .Where.Add("id", 6);

      using (var conn = db.CreateConnection())
      {
        conn.Open();

        var shop = conn.FetchRecord(select);
        Assert.False(shop.IsUpdated);

        var newName = Sdx.Util.String.GenRandom(10);
        var dateTime = Sdx.Util.Datetime.RoundTicks(DateTime.Now);
        shop.SetValue("name", newName);
        Assert.True(shop.IsUpdated);
        shop.SetValue("area_id", 2);
        shop.SetValue("main_image_id", 1);
        shop.SetValue("created_at", dateTime.ToString("yyyy-MM-dd HH:mm:ss"));

        Assert.Equal(newName, shop.GetString("name"));
        Assert.Equal(2, shop.GetInt32("area_id"));
        Assert.Equal(1, shop.GetInt32("main_image_id"));
        Assert.Equal(dateTime, shop.GetDateTime("created_at"));

        conn.BeginTransaction();
        conn.Save(shop);
        conn.Commit();

        Assert.False(shop.IsUpdated);

        Assert.Equal(newName, shop.GetString("name"));
        Assert.Equal(2, shop.GetInt32("area_id"));
        Assert.Equal(1, shop.GetInt32("main_image_id"));
        Assert.Equal(dateTime, shop.GetDateTime("created_at"));

        var updatedShop = conn.FetchRecord(select);
        Assert.Equal(newName, updatedShop.GetString("name"));
        Assert.Equal(2, updatedShop.GetInt32("area_id"));
        Assert.Equal(1, updatedShop.GetInt32("main_image_id"));
        Assert.Equal(DBNull.Value, updatedShop.GetValue("sub_image_id"));
        Assert.Equal(dateTime, updatedShop.GetDateTime("created_at"));
      }
    }

    [Fact]
    public void TestUpdateNull()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunUpdateNull(db);
      }
    }

    private void RunUpdateNull(TestDb testDb)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var db = testDb.Adapter;

      var insert = db.CreateInsert();

      insert
         .SetInto("shop")
         .AddColumnValue("name", "Baaaz")
         .AddColumnValue("area_id", 1)
         .AddColumnValue("login_id", "BaaazId")
         .AddColumnValue("created_at", DateTime.Now);

      object id = 0;
      using (var command = insert.Build())
      {
        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();

          conn.ExecuteNonQuery(command);

          id = conn.FetchLastInsertId();

          conn.Commit();
        }
      }

      var update = db.CreateUpdate();

      update
        .SetTable("shop")
        .AddColumnValue("login_id", DBNull.Value)
        .Where.Add("id", id);

      using (var command = update.Build())
      {
        Assert.Equal(testDb.Sql(@"UPDATE
          {0}shop{1}
          SET
            {0}login_id{1} = @0
          WHERE {0}id{1} = @1"), command.CommandText);

        Assert.Equal(DBNull.Value, command.Parameters["@0"].Value);
        Assert.Equal(id, command.Parameters["@1"].Value);

        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();
          conn.ExecuteNonQuery(command);
          conn.Commit();
        }
      }

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var select = db.CreateSelect();
        select
          .AddFrom("shop")
          .AddColumns("login_id")
          .WhereCall((where) => where.Add("name", "Baaaz"));

        var login_id = conn.FetchOne<object>(select);
        Assert.Equal(DBNull.Value, login_id);
      }
    }
  }
}
