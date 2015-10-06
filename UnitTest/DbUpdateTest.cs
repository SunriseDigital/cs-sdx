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
    public void TestInsert()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunInsert(db);
      }
    }

    private void RunInsert(TestDb testDb)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var db = testDb.Adapter;

      var insert = db.CreateInsert();

      var now = DateTime.Now;
      //mysqlで保存時にミリ秒は丸められてテストがこけるのでtruncateします。
      now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));
      insert
         .SetInto("shop")
         .AddColumnValue("name", "FooBar")
         .AddColumnValue("area_id", 1)
         .AddColumnValue("created_at", now);

      ulong id = 0;
      using (var command = insert.Build())
      {
        Assert.Equal(
          testDb.Sql("INSERT INTO {0}shop{1} ({0}name{1}, {0}area_id{1}, {0}created_at{1}) VALUES (@0, @1, @2)"),
          command.CommandText
        );

        Assert.Equal("FooBar", command.Parameters["@0"].Value);
        Assert.Equal(1, command.Parameters["@1"].Value);
        Assert.Equal(now, command.Parameters["@2"].Value);

        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();

          conn.ExecuteNonQuery(command);

          id = conn.FetchLastInsertId();

          conn.Commit();
        }
      }


      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var command = CreateFooBarCommand(conn, "FooBar");

        var shop = conn.FetchDictionary<string>(command);
        Assert.Equal(id.ToString(), shop["id"]);
        Assert.Equal("1", shop["area_id"]);

        Assert.Equal(now.ToString(), shop["created_at"]);
      }

      //ExecuteInsert
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        conn.Execute(insert);
        conn.Commit();
      }

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var command = conn.CreateCommand();
        command.CommandText = "SELECT * FROM shop WHERE name = @name";
        var param = command.CreateParameter();
        param.ParameterName = "@name";
        param.Value = "FooBar";
        command.Parameters.Add(param);

        var shops = conn.FetchDictionaryList<string>(command);
        Assert.Equal(2, shops.Count);
      }
    }

    private DbCommand CreateFooBarCommand(Sdx.Db.Connection conn, string name)
    {
      var command = conn.CreateCommand();
      command.CommandText = "SELECT * FROM shop WHERE name = @name";
      var param = command.CreateParameter();
      param.ParameterName = "@name";
      param.Value = name;
      command.Parameters.Add(param);

      return command;
    }

    [Fact]
    public void TestInsertWithSubquery()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunInsertWithSubquery(db);
      }
    }

    private void RunInsertWithSubquery(TestDb testDb)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var db = testDb.Adapter;

      //複数カラムのサブクエリ
      var insert = db.CreateInsert();

      insert
        .SetInto("shop")
        .AddColumn("name")
        .AddColumn("area_id")
        .AddColumn("created_at");

      var select = db.CreateSelect();
      select
        .AddColumn(Sdx.Db.Sql.Expr.Wrap("'FooBarSubquery'"))
        .AddFrom("shop")
        .AddColumns("area_id", "created_at")
        .Where.Add("id", 1)
        ;

      insert.SetSubquery(select);

      using (var command = insert.Build())
      {
        Assert.Equal(
          testDb.Sql(@"INSERT INTO {0}shop{1}
            ({0}name{1}, {0}area_id{1}, {0}created_at{1})
            (SELECT
              'FooBarSubquery',
              {0}shop{1}.{0}area_id{1},
              {0}shop{1}.{0}created_at{1}
            FROM {0}shop{1}
            WHERE {0}shop{1}.{0}id{1} = @0)"),
          command.CommandText
        );

        Assert.Equal(1, command.Parameters["@0"].Value);

        ulong id = 0;
        using (var conn = db.CreateConnection())
        {
          conn.Open();
          conn.BeginTransaction();

          conn.ExecuteNonQuery(command);

          id = conn.FetchLastInsertId();

          conn.Commit();
        }

        using (var conn = db.CreateConnection())
        {
          conn.Open();

          var shop = conn.FetchDictionary<string>(CreateFooBarCommand(conn, "FooBarSubquery"));
          Assert.Equal(id.ToString(), shop["id"]);
          Assert.Equal("2", shop["area_id"]);
        }
      }

      //単一カラムのサブクエリLiteral
      insert = db.CreateInsert();

      var now = DateTime.Now;
      insert
        .SetInto("shop")
        .AddColumnValue("name", "SimpleLiteralSub")
        .AddColumnValue("area_id", Sdx.Db.Sql.Expr.Wrap("(SELECT id FROM area WHERE id = 4)"))
        .AddColumnValue("created_at", now);

      using (var command = insert.Build())
      {
        Assert.Equal(testDb.Sql(@"INSERT
          INTO {0}shop{1}
            ({0}name{1}, {0}area_id{1}, {0}created_at{1})
          VALUES
            (@0, (SELECT id FROM area WHERE id = 4), @1)"), command.CommandText);

        Assert.Equal("SimpleLiteralSub", command.Parameters["@0"].Value);
        Assert.Equal(now, command.Parameters["@1"].Value);

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
        .Where.Add("name", "SimpleLiteralSub");

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);

        Assert.Equal("SimpleLiteralSub", shop.GetValue("name"));
        Assert.Equal(4, shop.GetValue("area_id"));
      }

      //単一カラムのサブクエリSelect
      insert = db.CreateInsert();

      select = db.CreateSelect();

      select
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", 5);

      now = DateTime.Now;
      insert
        .SetInto("shop")
        .AddColumnValue("name", "SimpleSelectSub")
        .AddColumnValue("area_id", select)
        .AddColumnValue("created_at", now);

      using (var command = insert.Build())
      {
        Assert.Equal(testDb.Sql(@"INSERT
          INTO {0}shop{1}
          ({0}name{1}, {0}area_id{1}, {0}created_at{1})
          VALUES (@0,
            (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}id{1} = @1),
            @2)"), command.CommandText);

        Assert.Equal("SimpleSelectSub", command.Parameters["@0"].Value);
        Assert.Equal(5, command.Parameters["@1"].Value);
        Assert.Equal(now, command.Parameters["@2"].Value);

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
        .Where.Add("name", "SimpleSelectSub");

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);

        Assert.Equal("SimpleSelectSub", shop.GetValue("name"));
        Assert.Equal(5, shop.GetValue("area_id"));
      }
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
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);

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
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);

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
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);

        Assert.Equal(4, shop.GetValue("area_id"));
      }
    }
  }
}
