﻿using Xunit;
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
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Query.Profiler();
      var db = testDb.Adapter;

      ulong id = 0;
      var now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        conn.Insert("shop", new Dictionary<string, object> {
          { "name", "FooBar" },
          { "area_id", "1"},
          { "created_at", now}
        });

        id = conn.FetchLastInsertId();

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

        var shop = conn.FetchDictionary<string>(command);
        Assert.Equal(id.ToString(), shop["id"]);
        Assert.Equal("1", shop["area_id"]);
        Assert.Equal(now, shop["created_at"]);
      }

      Sdx.Context.Current.DbProfiler.Queries.ForEach(query => {
        Console.WriteLine(query.ToString());
      });
      

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
