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
  public class Db_Sql_Insert : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
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

      //mysqlで保存時にミリ秒は丸められてテストがこけるのでtruncateします。
      var now = Sdx.Util.Datetime.RoundTicks(DateTime.Now);
      
      insert
         .SetInto("shop")
         .AddColumnValue("name", "FooBar")
         .AddColumnValue("area_id", 1)
         .AddColumnValue("created_at", now);

      object id = 0;
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

        object id = 0;
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
        var shop = conn.FetchRecord(select);

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
        var shop = conn.FetchRecord(select);

        Assert.Equal("SimpleSelectSub", shop.GetValue("name"));
        Assert.Equal(5, shop.GetValue("area_id"));
      }
    }

    [Fact]
    public void TestInsertNull()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunInsertNull(db);
      }
    }

    private void RunInsertNull(TestDb testDb)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var db = testDb.Adapter;

      var insert = db.CreateInsert();

      insert
         .SetInto("shop")
         .AddColumnValue("name", "Baaaz")
         .AddColumnValue("area_id", 1)
         .AddColumnValue("login_id", DBNull.Value)
         .AddColumnValue("created_at", DateTime.Now);

      object id = 0;
      using (var command = insert.Build())
      {
        Assert.Equal(
          testDb.Sql("INSERT INTO {0}shop{1} ({0}name{1}, {0}area_id{1}, {0}login_id{1}, {0}created_at{1}) VALUES (@0, @1, @2, @3)"),
          command.CommandText
        );

        Assert.Equal("Baaaz", command.Parameters["@0"].Value);
        Assert.Equal(1, command.Parameters["@1"].Value);
        Assert.Equal(DBNull.Value, command.Parameters["@2"].Value);

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
