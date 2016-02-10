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
  public class DbConnectionTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestCreate()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunCreate(db);
      }
    }

    private void RunCreate(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();

      select
        .SetLimit(1)
        .AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        var command = select.Build();
        var shop = conn.FetchDictionary(command);
        Assert.True(conn.IsAttachedTo(command));
        conn.Commit();
      }

      //commandにちゃんとConnectionとTransactionが代入されてるかのテストだったが、readerを閉じ忘れてるバグを発見したので全Fetch系をテストします。
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        var command = select.Build();
        var id = conn.FetchOne(command);
        Assert.True(conn.IsAttachedTo(command));
        conn.Commit();
      }

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        var command = select.Build();
        var result = conn.FetchList(command);
        Assert.True(conn.IsAttachedTo(command));
        conn.Commit();
      }

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        var command = select.Build();
        var result = conn.FetchKeyValuePairList<string, string>(command);
        Assert.True(conn.IsAttachedTo(command));
        conn.Commit();
      }

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        var command = select.Build();
        var result = conn.FetchDictionaryList(command);
        Assert.True(conn.IsAttachedTo(command));
        conn.Commit();
      }
    }

    [Fact]
    public void TestQueryLog()
    {
      var db = this.CreateTestDbList()[0].Adapter;


      var profiler = new Sdx.Db.Sql.Profiler();
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
        var reader = con.ExecuteReader(command);
      }

      Assert.Equal(3, profiler.Logs.Count);
      Assert.True(profiler.Logs[0].ElapsedTime > 0);
      Assert.True(profiler.Logs[0].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id", profiler.Logs[1].CommandText);
      Assert.Equal("@id : 1", profiler.Logs[1].FormatedParameters);


      command.CommandText = "SELECT * FROM shop WHERE id > @id AND name = @name";
      param = command.CreateParameter();
      param.ParameterName = "@name";
      param.Value = "foobar";
      command.Parameters.Add(param);

      using (var con = db.CreateConnection())
      {
        con.Open();
        var reader = con.ExecuteReader(command);
      }

      Assert.Equal(6, profiler.Logs.Count);
      Assert.True(profiler.Logs[1].ElapsedTime > 0);
      Assert.True(profiler.Logs[1].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id AND name = @name", profiler.Logs[4].CommandText);
      Assert.Equal("  @id : 1" + System.Environment.NewLine + "@name : foobar", profiler.Logs[4].FormatedParameters);
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var list = conn.FetchDictionaryList(sel.Build());
        Assert.IsType<List<Dictionary<string, object>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天祥", list[0]["name"]);
        Assert.Equal(DBNull.Value, list[0]["main_image_id"]);
        Assert.Equal("エスペリア", list[1]["name"]);
      }



      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where
          .Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var list = conn.FetchDictionaryList(sel);
        Assert.IsType<List<Dictionary<string, object>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天府舫", list[0]["name"]);
        Assert.Equal("Freeve", list[1]["name"]);
      }
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("id", "name")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;


      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var list = conn.FetchKeyValuePairList<int, string>(sel.Build());
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
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id", "name", "main_image_id")//最初の二つ以外は無視
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var list = conn.FetchKeyValuePairList<int, string>(sel);
        Assert.IsType<List<KeyValuePair<int, string>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal(3, list[0].Key);
        Assert.Equal("天府舫", list[0].Value);
        Assert.Equal(4, list[1].Key);
        Assert.Equal("Freeve", list[1].Value);
      }
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var list = conn.FetchList(sel.Build());
        Assert.IsType<List<object>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天祥", list[0]);
        Assert.Equal("エスペリア", list[1]);
      }


      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("name", "main_image_id")//最初１つ以外は無視
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var list = conn.FetchList(sel);
        Assert.IsType<List<object>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天府舫", list[0]);
        Assert.Equal("Freeve", list[1]);
      }

      //int
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 4, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var intList = conn.FetchList(sel);
        Assert.IsType<List<object>>(intList);
        Assert.Equal(2, intList.Count);
        Assert.Equal(5, intList[0]);
        Assert.Equal(6, intList[1]);
      }


      //datetime
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .AddColumns("created_at")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC);
      sel
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var datetimes = conn.FetchList(sel);
        Assert.IsType<List<object>>(datetimes);
        Assert.Equal(2, datetimes.Count);
        Assert.Equal("2015-01-01 12:30:00", ((DateTime)datetimes[0]).ToString("yyyy-MM-dd HH:mm:ss"));
        Assert.Equal("2015-01-02 12:30:00", ((DateTime)datetimes[1]).ToString("yyyy-MM-dd HH:mm:ss"));
      }
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)//２行目以降は無視
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var intValue = conn.FetchOne(sel.Build());
        Assert.IsType<int>(intValue);
        Assert.Equal(1, intValue);
      }


      //string
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("name", "main_image_id")//二つ目以降は無視
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var strValue = conn.FetchOne(sel);
        Assert.IsType<string>(strValue);
        Assert.Equal("天府舫", strValue);
      }



      //string
      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 3, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("created_at", "main_image_id")//二つ目以降は無視
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var dtValue = conn.FetchOne(sel);
        Assert.IsType<DateTime>(dtValue);
        Assert.Equal("2015-01-04 12:30:00", ((DateTime)dtValue).ToString("yyyy-MM-dd HH:mm:ss"));
      }
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
        .AddFrom("shop");
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var strDic = conn.FetchDictionary(sel.Build());
        Assert.IsType<Dictionary<string, object>>(strDic);
        Assert.Equal(1, strDic["id"]);
        Assert.Equal("天祥", strDic["name"]);
        Assert.Equal(DBNull.Value, strDic["main_image_id"]);
      }

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var objDic = conn.FetchDictionary(sel);
        Assert.IsType<Dictionary<string, object>>(objDic);
        Assert.Equal(3, objDic["id"]);
        Assert.Equal("天府舫", objDic["name"]);
        Assert.IsType<DBNull>(objDic["main_image_id"]);
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
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .Table.SetColumns("id");

      using (var con = db.Adapter.CreateConnection())
      {
        object id = null;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          id = con.FetchOne(select);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        id = con.FetchOne(select);
        Assert.Equal(1, id);
        Assert.Equal(System.Data.ConnectionState.Open, con.State);
      }

      using (var con = db.Adapter.CreateConnection())
      {
        var command = select.Build();

        object id = null;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          id = con.FetchOne(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        id = con.FetchOne(command);
        Assert.Equal(1, id);
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      List<object> list = null;

      using (var con = db.Adapter.CreateConnection())
      {
        var command = sel.Build();

        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          list = con.FetchList(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = con.FetchList(command);
        Assert.IsType<List<object>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天祥", list[0]);
        Assert.Equal("エスペリア", list[1]);
      }


      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("name", "main_image_id")//最初１つ以外は無視
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          list = con.FetchList(sel);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = con.FetchList(sel);
        Assert.IsType<List<object>>(list);
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Dictionary<string, object> strDic = null;
        var command = sel.Build();
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          strDic = con.FetchDictionary(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        strDic = con.FetchDictionary(sel.Build());
        Assert.IsType<Dictionary<string, object>>(strDic);
        Assert.Equal(1, strDic["id"]);
        Assert.Equal("天祥", strDic["name"]);
        Assert.Equal(DBNull.Value, strDic["main_image_id"]);
      }


      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Dictionary<string, object> objDic;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          objDic = con.FetchDictionary(sel);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        objDic = con.FetchDictionary(sel);
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;
      List<Dictionary<string, object>> list = null;

      using (var con = db.Adapter.CreateConnection())
      {
        var command = sel.Build();
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          list = con.FetchDictionaryList(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        command.Connection.Open();
        list = con.FetchDictionaryList(command);
        Assert.IsType<List<Dictionary<string, object>>>(list);
        Assert.Equal(2, list.Count);
        Assert.Equal("天祥", list[0]["name"]);
        Assert.Equal(DBNull.Value, list[0]["main_image_id"]);
        Assert.Equal("エスペリア", list[1]["name"]);
      }

      sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop")
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id", "name", "main_image_id")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          list = con.FetchDictionaryList(sel);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = con.FetchDictionaryList(sel);
        Assert.IsType<List<Dictionary<string, object>>>(list);
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom("shop");
      sel
        .AddColumns("id", "name")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      List<KeyValuePair<int, string>> list = null;
      using (var con = db.Adapter.CreateConnection())
      {
        var command = sel.Build();
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          list = con.FetchKeyValuePairList<int, string>(command);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = con.FetchKeyValuePairList<int, string>(command);
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
        .Where.Add("id", 2, Sdx.Db.Sql.Comparison.GreaterThan);
      sel
        .AddColumns("id", "name", "main_image_id")//最初の二つ以外は無視
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(2)
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          list = con.FetchKeyValuePairList<int, string>(sel);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        list = con.FetchKeyValuePairList<int, string>(sel);
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
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", 1);

      using (var con = db.Adapter.CreateConnection())
      {
        Test.Orm.Shop shop;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          shop = (Test.Orm.Shop)con.FetchRecord(sel);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        shop = (Test.Orm.Shop)con.FetchRecord(sel);
        Assert.Equal(1, shop.GetInt32("id"));
        Assert.Equal("天祥", shop.GetString("name"));
      }
    }

    [Fact]
    public void TestFetchRecordSetWithConnection()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchRecordSetWithConnection(db);
      }
    }

    private void RunFetchRecordSetWithConnection(TestDb db)
    {
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Sql.Order.DESC)
        .Where
          .Add("id", new string[] { "2", "3" })
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        Sdx.Db.RecordSet set;
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          set = con.FetchRecordSet(sel);
        }));
        //connectionを開いてないので例外になるはず
        Assert.Equal(typeof(Sdx.Db.DbException), ex.GetType());

        con.Open();
        set = con.FetchRecordSet(sel);
        Assert.Equal(2, set.Count);
        Assert.Equal(3, set[0].GetInt32("id"));
        Assert.Equal("天府舫", set[0].GetString("name"));
        Assert.Equal(2, set[1].GetInt32("id"));
        Assert.Equal("エスペリア", set[1].GetString("name"));
      }
    }

    [Fact]
    public void TestProfiler()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunProfiler(db);
      }
    }

    private void RunProfiler(TestDb testDb)
    {
      var db = testDb.Adapter;
      var context = Sdx.Context.Current;
      var prevProfiler = context.DbProfiler;
      context.DbProfiler = new Sdx.Db.Sql.Profiler();

      using (var conn = db.CreateConnection())
      {
        conn.Open();

        Assert.Equal(1, context.DbProfiler.Logs.Count);
        Assert.Equal("OPEN", context.DbProfiler.Logs[0].CommandText);

        conn.BeginTransaction();
        Assert.Equal(2, context.DbProfiler.Logs.Count);
        Assert.Equal("BEGIN TRANSACTION", context.DbProfiler.Logs[1].CommandText);

        conn.Commit();
        Assert.Equal(3, context.DbProfiler.Logs.Count);
        Assert.Equal("COMMIT", context.DbProfiler.Logs[2].CommandText);
      }

      Assert.Equal(4, context.DbProfiler.Logs.Count);
      Assert.Equal("CLOSE", context.DbProfiler.Logs[3].CommandText);

      context.DbProfiler = new Sdx.Db.Sql.Profiler();

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        conn.Rollback();
        Assert.Equal(3, context.DbProfiler.Logs.Count);
        Assert.Equal("ROLLBACK", context.DbProfiler.Logs[2].CommandText);
      }

      context.DbProfiler = prevProfiler;
    }

    [Fact]
    public void TestFetchRecordSetNonGeneric()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchRecordSetNonGeneric(db);
      }
    }

    private void RunFetchRecordSetNonGeneric(TestDb db)
    {
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Sql.Order.DESC)
        .Where
          .Add("id", new string[] { "2", "3" })
        ;

      using (var con = db.Adapter.CreateConnection())
      {
        dynamic set;
        con.Open();
        set = con.FetchRecordSet(sel);
        Assert.Equal(2, set.Count);
        Assert.Equal(3, set[0].GetInt32("id"));
        Assert.Equal("天府舫", set[0].GetString("name"));
        Assert.Equal(2, set[1].GetInt32("id"));
        Assert.Equal("エスペリア", set[1].GetString("name"));
      }
    }

    [Fact]
    public void TestFetchRecordNonGeneric()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchRecordNonGeneric(db);
      }
    }

    private void RunFetchRecordNonGeneric(TestDb db)
    {
      var sel = db.Adapter.CreateSelect();
      sel
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", 1);

      using (var con = db.Adapter.CreateConnection())
      {
        con.Open();
        Sdx.Db.Record shop = con.FetchRecord(sel);
        Assert.Equal(1, shop.GetInt32("id"));
        Assert.Equal("天祥", shop.GetString("name"));
      }
    }

  }
}
