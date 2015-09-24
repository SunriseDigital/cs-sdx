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


      var profiler = new Sdx.Db.Query.Profiler();
      Sdx.Context.Current.DbProfiler = profiler;

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

      Assert.Equal(1, profiler.Queries.Count);
      Assert.True(profiler.Queries[0].ElapsedTime > 0);
      Assert.True(profiler.Queries[0].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id", profiler.Queries[0].CommandText);
      Assert.Equal("@id : 1", profiler.Queries[0].FormatedParameters);


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

      Assert.Equal(2, profiler.Queries.Count);
      Assert.True(profiler.Queries[1].ElapsedTime > 0);
      Assert.True(profiler.Queries[1].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id AND name = @name", profiler.Queries[1].CommandText);
      Assert.Equal("  @id : 1" + System.Environment.NewLine + "@name : foobar", profiler.Queries[1].FormatedParameters);
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

      var list = db.Adapter.FetchDictionaryList<string>(sel.Build());
      Assert.IsType<List<Dictionary<string, string>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal("天祥", list[0]["name"]);
      Assert.Equal("", list[0]["main_image_id"]);
      Assert.Equal("エスペリア", list[1]["name"]);

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      list = db.Adapter.FetchDictionaryList<string>(sel);
      Assert.IsType<List<Dictionary<string, string>>>(list);
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

      var list = db.Adapter.FetchKeyValuePairList<int, string>(sel.Build());
      Assert.IsType<List<KeyValuePair<int, string>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal(1, list[0].Key);
      Assert.Equal("天祥", list[0].Value);
      Assert.Equal(2, list[1].Key);
      Assert.Equal("エスペリア", list[1].Value);


      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")//最初の二つ以外は無視
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      list = db.Adapter.FetchKeyValuePairList<int, string>(sel);
      Assert.IsType<List<KeyValuePair<int, string>>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal(3, list[0].Key);
      Assert.Equal("天府舫", list[0].Value);
      Assert.Equal(4, list[1].Key);
      Assert.Equal("Freeve", list[1].Value);
    }

    [Fact]
    public void TestFetchList()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchList(db);
      }
    }

    private void RunFetchList(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var list = db.Adapter.FetchList<string>(sel.Build());
      Assert.IsType<List<string>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal("天祥", list[0]);
      Assert.Equal("エスペリア", list[1]);

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("name", "main_image_id")//最初１つ以外は無視
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      list = sel.FetchList<string>();
      Assert.IsType<List<string>>(list);
      Assert.Equal(2, list.Count);
      Assert.Equal("天府舫", list[0]);
      Assert.Equal("Freeve", list[1]);

      //int
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 4, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var intList = sel.FetchList<int>();
      Assert.IsType<List<int>>(intList);
      Assert.Equal(2, intList.Count);
      Assert.Equal(5, intList[0]);
      Assert.Equal(6, intList[1]);

      //datetime
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .AddColumns("created_at")
        .AddOrder("id", Sdx.Db.Query.Order.ASC).Select
        .SetLimit(2)
        ;

      var datetimes = sel.FetchList<DateTime>();
      Assert.IsType<List<DateTime>>(datetimes);
      Assert.Equal(2, datetimes.Count);
      Assert.Equal("2015-01-01 12:30:00", datetimes[0].ToString("yyyy-MM-dd HH:mm:ss"));
      Assert.Equal("2015-01-02 12:30:00", datetimes[1].ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public void TestFetchOne()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchOne(db);
      }
    }

    private void RunFetchOne(TestDb db)
    {
      //int
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)//２行目以降は無視
        ;

      var intValue = db.Adapter.FetchOne<int>(sel.Build());
      Assert.IsType<int>(intValue);
      Assert.Equal(1, intValue);

      //string
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("name", "main_image_id")//二つ目以降は無視
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var strValue = sel.FetchOne<string>();
      Assert.IsType<string>(strValue);
      Assert.Equal("天府舫", strValue);


      //string
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 3, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("created_at", "main_image_id")//二つ目以降は無視
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var dtValue = sel.FetchOne<DateTime>();
      Assert.IsType<DateTime>(dtValue);
      Assert.Equal("2015-01-04 12:30:00", dtValue.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public void TestFetchDictionary()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchDictionary(db);
      }
    }

    private void RunFetchDictionary(TestDb db)
    {
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop").Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var strDic = db.Adapter.FetchDictionary<string>(sel.Build());
      Assert.IsType<Dictionary<string, string>>(strDic);
      Assert.Equal("1", strDic["id"]);
      Assert.Equal("天祥", strDic["name"]);
      Assert.Equal("", strDic["main_image_id"]);

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      var objDic = sel.FetchDictionary<object>();
      Assert.IsType<Dictionary<string, object>>(objDic);
      Assert.Equal(3, objDic["id"]);
      Assert.Equal("天府舫", objDic["name"]);
      Assert.IsType<DBNull>(objDic["main_image_id"]);
    }

    [Fact]
    public void TestSecureConnectionString()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        Assert.NotEqual(-1, db.Adapter.ToString().IndexOf(Sdx.Db.Adapter.PWD_FOR_SECURE_CONNECTION_STRING));
      }
    }

    [Fact]
    public void TestFetchOneWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchOneWithConnection(db);
      }
    }

    private void RunFetchOneWithConnection(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .Table.SetColumns("id");

      using (var con = db.Adapter.CreateConnection())
      {
        string id = null;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          id = select.FetchOne<string>(con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        id = select.FetchOne<string>(con);
        Assert.Equal("1", id);
        Assert.Equal(System.Data.ConnectionState.Open, con.State);
      }

      using (var con = db.Adapter.CreateConnection())
      {
        select.Adapter = db.Adapter;
        var command = select.Build();
        command.Connection = con;

        string id = null;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          id = db.Adapter.FetchOne<string>(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        id = db.Adapter.FetchOne<string>(command);
        Assert.Equal("1", id);
      }
    }

    [Fact]
    public void TestFetchListWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchListWithConnection(db);
      }
    }

    private void RunFetchListWithConnection(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      List<string> list = null;

      using (var con = db.Adapter.CreateConnection())
      {
        var command = sel.Build();
        command.Connection = con;

        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          list = db.Adapter.FetchList<string>(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = db.Adapter.FetchList<string>(command);
        Assert.IsType<List<string>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天祥", list[0]);
        Assert.Equal("エスペリア", list[1]);
      }


      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("name", "main_image_id")//最初１つ以外は無視
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          list = sel.FetchList<string>(con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = sel.FetchList<string>(con);
        Assert.IsType<List<string>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天府舫", list[0]);
        Assert.Equal("Freeve", list[1]);
      }
    }

    [Fact]
    public void TestFetchDictionaryWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchDictionaryWithConnection(db);
      }
    }

    private void RunFetchDictionaryWithConnection(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Dictionary<string, string> strDic = null;
        var command = sel.Build();
        command.Connection = con;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          strDic = db.Adapter.FetchDictionary<string>(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        strDic = db.Adapter.FetchDictionary<string>(sel.Build());
        Assert.IsType<Dictionary<string, string>>(strDic);
        Assert.Equal("1", strDic["id"]);
        Assert.Equal("天祥", strDic["name"]);
        Assert.Equal("", strDic["main_image_id"]);
      }


      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Dictionary<string, object> objDic;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          objDic = sel.FetchDictionary<object>(con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        objDic = sel.FetchDictionary<object>(con);
        Assert.IsType<Dictionary<string, object>>(objDic);
        Assert.Equal(3, objDic["id"]);
        Assert.Equal("天府舫", objDic["name"]);
        Assert.IsType<DBNull>(objDic["main_image_id"]);
      }
    }

    [Fact]
    public void TestFetchDictionaryListWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchDictionaryListWithConnection(db);
      }
    }

    private void RunFetchDictionaryListWithConnection(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;
      List<Dictionary<string, string>> list = null;

      using (var con = db.Adapter.CreateConnection())
      {
        var command = sel.Build();
        command.Connection = con;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          list = db.Adapter.FetchDictionaryList<string>(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        command.Connection.Open();
        list = db.Adapter.FetchDictionaryList<string>(command);
        Assert.IsType<List<Dictionary<string, string>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天祥", list[0]["name"]);
        Assert.Equal("", list[0]["main_image_id"]);
        Assert.Equal("エスペリア", list[1]["name"]);
      }

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          list = db.Adapter.FetchDictionaryList<string>(sel, con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = db.Adapter.FetchDictionaryList<string>(sel, con);
        Assert.IsType<List<Dictionary<string, string>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天府舫", list[0]["name"]);
        Assert.Equal("Freeve", list[1]["name"]);
      }
    }

    [Fact]
    public void TestFetchKeyValuePairListWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchKeyValuePairListWithConnection(db);
      }
    }

    private void RunFetchKeyValuePairListWithConnection(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom("shop").Select
        .AddColumns("id", "name")
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      List<KeyValuePair<int, string>> list = null;
      using (var con = db.Adapter.CreateConnection())
      {
        var command = sel.Build();
        command.Connection = con;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          list = db.Adapter.FetchKeyValuePairList<int, string>(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = db.Adapter.FetchKeyValuePairList<int, string>(command);
        Assert.IsType<List<KeyValuePair<int, string>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0].Key);
        Assert.Equal("天祥", list[0].Value);
        Assert.Equal(2, list[1].Key);
        Assert.Equal("エスペリア", list[1].Value);
      }

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Query.Comparison.GreaterThan).Context.Select
        .AddColumns("id", "name", "main_image_id")//最初の二つ以外は無視
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          list = db.Adapter.FetchKeyValuePairList<int, string>(sel, con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = db.Adapter.FetchKeyValuePairList<int, string>(sel, con);
        Assert.IsType<List<KeyValuePair<int, string>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal(3, list[0].Key);
        Assert.Equal("天府舫", list[0].Value);
        Assert.Equal(4, list[1].Key);
        Assert.Equal("Freeve", list[1].Value);
      }
    }

    [Fact]
    public void TestFetchRecordWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchRecordWithConnection(db);
      }
    }

    private void RunFetchRecordWithConnection(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", 1);

      using (var con = db.Adapter.CreateConnection())
      {
        Test.Orm.Shop shop;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          shop = db.Adapter.FetchRecord<Test.Orm.Shop>(sel, con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        shop = db.Adapter.FetchRecord<Test.Orm.Shop>(sel, con);
        Assert.Equal(1, shop.GetInt32("id"));
        Assert.Equal("天祥", shop.GetString("name"));
      }
    }

    [Fact]
    public void TestFetchRecordListWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchRecordListWithConnection(db);
      }
    }

    private void RunFetchRecordListWithConnection(TestDb db)
    {
      var sel = new Sdx.Db.Query.Select(db.Adapter);
      sel
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", new string[] { "2", "3" }).Context
        .AddOrder("id", Sdx.Db.Query.Order.DESC)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Sdx.Db.RecordSet<Test.Orm.Shop> set;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          set = db.Adapter.FetchRecordSet<Test.Orm.Shop>(sel, con);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        set = db.Adapter.FetchRecordSet<Test.Orm.Shop>(sel, con);
        Assert.Equal(2, set.Count);
        Assert.Equal(3, set[0].GetInt32("id"));
        Assert.Equal("天府舫", set[0].GetString("name"));
        Assert.Equal(2, set[1].GetInt32("id"));
        Assert.Equal("エスペリア", set[1].GetString("name"));
      }
    }
  }
}
