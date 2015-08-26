using Xunit;
using UnitTest.DummyClasses;
using System.Collections.Generic;

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
  public class DbAdapterTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestQueryLog()
    {
      var db = this.CreateTestDbList()[0].Adapter;

      db.Profiler = new Sdx.Db.Query.Profiler();

      var command = db.CreateCommand();
      command.CommandText = "SELECT * FROM shop WHERE id > @id";

      var param = command.CreateParameter();
      param.ParameterName = "@id";
      param.Value = 1;
      command.Parameters.Add(param);

      using (var con = db.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = db.ExecuteReader(command);
      }

      Assert.Equal(1, db.Profiler.Queries.Count);
      Assert.True(db.Profiler.Queries[0].ElapsedTime > 0);
      Assert.True(db.Profiler.Queries[0].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id", db.Profiler.Queries[0].CommandText);
      Assert.Equal("@id : 1", db.Profiler.Queries[0].FormatedParameters);


      command.CommandText = "SELECT * FROM shop WHERE id > @id AND name = @name";
      param = command.CreateParameter();
      param.ParameterName = "@name";
      param.Value = "foobar";
      command.Parameters.Add(param);

      using (var con = db.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = db.ExecuteReader(command);
      }

      Assert.Equal(2, db.Profiler.Queries.Count);
      Assert.True(db.Profiler.Queries[1].ElapsedTime > 0);
      Assert.True(db.Profiler.Queries[1].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id AND name = @name", db.Profiler.Queries[1].CommandText);
      Assert.Equal("  @id : 1"+System.Environment.NewLine+"@name : foobar", db.Profiler.Queries[1].FormatedParameters);
    }

    [Fact]
    public void TestFetchDictionaryList()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchDictionaryList(db);
      }
    }

    private void RunFetchDictionaryList(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var list = db.Adapter.FetchDictionaryList(sel.Build());
      Assert.IsType<List<Dictionary<string, object>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal("天祥", list[0]["name"]);
      Assert.Equal("", list[0]["main_image_id"].ToString());
      Assert.Equal("エスペリア", list[1]["name"]);

      sel = new Sdx.Db.Query.Select();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      list = db.Adapter.FetchDictionaryList(sel);
      Assert.IsType<List<Dictionary<string, object>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal("天府舫", list[0]["name"]);
      Assert.Equal("Freeve", list[1]["name"]);
    }

    [Fact]
    public void TestFetchKeyValuePairList()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchKeyValuePairList(db);
      }
    }

    private void RunFetchKeyValuePairList(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("id", "name")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var list = db.Adapter.FetchKeyValuePairList(sel.Build());
      Assert.IsType<List<KeyValuePair<object, object>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal(1, list[0].Key);
      Assert.Equal("天祥", list[0].Value);
      Assert.Equal(2, list[1].Key);
      Assert.Equal("エスペリア", list[1].Value);


      sel = new Sdx.Db.Query.Select();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      list = db.Adapter.FetchKeyValuePairList(sel);
      Assert.IsType<List<KeyValuePair<object, object>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal(3, list[0].Key);
      Assert.Equal("天府舫", list[0].Value);
      Assert.Equal(4, list[1].Key);
      Assert.Equal("Freeve", list[1].Value);
    }
  }
}
