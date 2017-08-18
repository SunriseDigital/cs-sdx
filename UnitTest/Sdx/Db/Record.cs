using Xunit;
using UnitTest.DummyClasses;
using System.Collections.Generic;
using System.Linq;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;
using System.Collections.Specialized;

namespace UnitTest
{
  [TestClass]
  public class Db_Record : BaseDbTest
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
         .AddOrder("id", Sdx.Db.Sql.Order.ASC);
      select.SetLimit(1);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, shops.Count);
        shops.ForEach(shop =>
        {
          Assert.Equal("1", shop.GetString("id"));
          Assert.Equal("天祥", shop.GetString("name"));
          Assert.Equal("2", shop.GetString("area_id"));
          Assert.Equal("", shop.GetString("main_image_id"));
          Assert.Equal("", shop.GetString("sub_image_id"));
        });
      }
    }

    [Fact]
    public void TestJoinManyOne()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunJoinManyOne(db);
        ExecSql(db);
      }
    }

    private void RunJoinManyOne(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();

      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         .InnerJoin(new Test.Orm.Table.Area())
         ;
      select.SetLimit(2);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(2, shops.Count);

        Assert.Equal("1", shops[0].GetString("id"));
        Assert.Equal("天祥", shops[0].GetString("name"));
        Assert.Equal("2", shops[0].GetString("area_id"));
        Assert.Equal("", shops[0].GetString("main_image_id"));
        Assert.Equal("", shops[0].GetString("sub_image_id"));

        var area = shops[0].GetRecordSet("area")[0];
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal("新中野", area.GetString("name"));

        Assert.Equal("2", shops[1].GetString("id"));
        Assert.Equal("エスペリア", shops[1].GetString("name"));
        Assert.Equal("3", shops[1].GetString("area_id"));
        Assert.Equal("", shops[1].GetString("main_image_id"));
        Assert.Equal("", shops[1].GetString("sub_image_id"));

        area = shops[1].GetRecordSet("area").First;
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal("西麻布", area.GetString("name"));
      }
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
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         ;

      select.Context("shop")
         .InnerJoin(new Test.Orm.Table.Menu())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         ;

      select.Context("shop")
          .Where.Add("name", "天府舫");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(1, shops.Count);
        Assert.Equal("天府舫", shops[0].GetString("name"));

        var menuList = shops[0].GetRecordSet("menu");
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.Equal(3, menuList.Count);
        Assert.Equal("干し豆腐のサラダ", menuList[0].GetString("name"));
        Assert.Equal("麻婆豆腐", menuList[1].GetString("name"));
        Assert.Equal("牛肉の激辛水煮", menuList[2].GetString("name"));
      }
    }

    [Fact]
    public void TestJoinManyMany()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunJoinManyMany(db);
        ExecSql(db);
      }
    }

    private void RunJoinManyMany(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();

      var select = db.Adapter.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .Where
          .Add("name", new List<string>() { "ビーナスラッシュ", "Freeve" });
      select.Context("shop")
        .InnerJoin(new Test.Orm.Table.ShopCategory())
         ;

      select.Context("shop_category").InnerJoin(new Test.Orm.Table.Category())
        .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(2, shops.Count);

        Assert.Equal("Freeve", shops[0].GetString("name"));
        Assert.Equal(3, shops[0].GetRecordSet("shop_category").Count);
        Assert.Equal("美容室", shops[0].GetRecordSet("shop_category")[0].GetRecordSet("category").First.GetString("name"));
        Assert.Equal("ネイルサロン", shops[0].GetRecordSet("shop_category")[1].GetRecordSet("category").First.GetString("name"));
        Assert.Equal("まつげエクステ", shops[0].GetRecordSet("shop_category")[2].GetRecordSet("category").First.GetString("name"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal("ビーナスラッシュ", shops[1].GetString("name"));
        Assert.Equal(2, shops[1].GetRecordSet("shop_category").Count);
        Assert.Equal("ネイルサロン", shops[0].GetRecordSet("shop_category")[1].GetRecordSet("category").First.GetString("name"));
        Assert.Equal("まつげエクステ", shops[0].GetRecordSet("shop_category")[2].GetRecordSet("category").First.GetString("name"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
      }
    }

    [Fact]
    public void TestNoJoinManyOne()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunNoJoinManyOne(db);
        ExecSql(db);
      }
    }

    private void RunNoJoinManyOne(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         ;

      select.SetLimit(1);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        var areaSet = shops[0].GetRecordSet("area", conn);
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(1, areaSet.Count);
        Assert.Equal("新中野", areaSet[0].GetString("name"));

        var largeAreaSet = areaSet[0].GetRecordSet("large_area", conn);
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(1, largeAreaSet.Count);
        Assert.Equal("東京", largeAreaSet[0].GetString("name"));
      }
    }

    [Fact]
    public void TestNoJoinOneMany()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunNoJoinOneMany(db);
        ExecSql(db);
      }
    }

    private void RunNoJoinOneMany(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         .Where.Add("name", "天府舫")
         ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        var menuSet = shops[0].GetRecordSet(
          "menu",
          conn,
          sel => { sel.Context("menu").AddOrder("id", Sdx.Db.Sql.Order.ASC); }
        );
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(3, menuSet.Count);
        Assert.Equal("干し豆腐のサラダ", menuSet[0].GetString("name"));
        Assert.Equal("麻婆豆腐", menuSet[1].GetString("name"));
        Assert.Equal("牛肉の激辛水煮", menuSet[2].GetString("name"));

        //ORDERをひっくり返す
        menuSet = shops[0].ClearRecordCache("menu").GetRecordSet(
          "menu",
          conn,
          sel => { sel.Context("menu").AddOrder("id", Sdx.Db.Sql.Order.DESC); }
        );
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(3, menuSet.Count);
        Assert.Equal("牛肉の激辛水煮", menuSet[0].GetString("name"));
        Assert.Equal("麻婆豆腐", menuSet[1].GetString("name"));
        Assert.Equal("干し豆腐のサラダ", menuSet[2].GetString("name"));
      }
    }


    [Fact]
    public void TestNoJoinManyMany()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunNoJoinManyMany(db);
        ExecSql(db);
      }
    }

    private void RunNoJoinManyMany(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         .Where.Add("name", "Freeve")
         ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        var shopCategorySet = shops[0].GetRecordSet(
          "shop_category",
          conn,
          sel => { sel.AddOrder("category_id", Sdx.Db.Sql.Order.ASC); }
        );
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(3, shopCategorySet.Count);
        Assert.Equal("美容室", shopCategorySet[0].GetRecordSet("category", conn).First.GetString("name"));
        Assert.Equal("ネイルサロン", shopCategorySet[1].GetRecordSet("category", conn).First.GetString("name"));
        Assert.Equal("まつげエクステ", shopCategorySet[2].GetRecordSet("category", conn).First.GetString("name"));
        Assert.Equal(5, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        shopCategorySet = shops[0].ClearRecordCache("shop_category").GetRecordSet(
          "shop_category",
          conn,
          sel => { sel.AddOrder("category_id", Sdx.Db.Sql.Order.DESC); }
        );
        Assert.Equal(6, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);


        Assert.Equal(3, shopCategorySet.Count);
        Assert.Equal("まつげエクステ", shopCategorySet[0].GetRecordSet("category", conn).First.GetString("name"));
        Assert.Equal("ネイルサロン", shopCategorySet[1].GetRecordSet("category", conn).First.GetString("name"));
        Assert.Equal("美容室", shopCategorySet[2].GetRecordSet("category", conn).First.GetString("name"));
        Assert.Equal(9, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
      }
    }

    [Fact]
    public void TestSameTableLeftJoinNotNull()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSameTableLeftJoinNotNull(db);
        ExecSql(db);
      }
    }

    private void RunSameTableLeftJoinNotNull(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      var cShop = select.AddFrom(new Test.Orm.Table.Shop());
      cShop.LeftJoin(new Test.Orm.Table.Image(), "main_image");
      cShop.LeftJoin(new Test.Orm.Table.Image(), "sub_image");
      cShop.Where.Add("name", "Freeve");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(1, shops.Count);

        Assert.Equal("/freeve/main.jpq", shops[0].GetRecord("main_image").GetString("path"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal("/freeve/sub.jpq", shops[0].GetRecord("sub_image").GetString("path"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
      }
    }

    [Fact]
    public void TestSameTableLeftJoinNull()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSameTableLeftJoinNull(db);
        ExecSql(db);
      }
    }

    private void RunSameTableLeftJoinNull(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      var cShop = select.AddFrom(new Test.Orm.Table.Shop());
      cShop.LeftJoin(new Test.Orm.Table.Image(), "main_image");
      cShop.LeftJoin(new Test.Orm.Table.Image(), "sub_image");
      cShop.Where.Add("name", "ビーナスラッシュ");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, shops.Count);

        Assert.Null(shops[0].GetRecord("main_image"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Null(shops[0].GetRecord("sub_image"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
      }
    }

    [Fact]
    public void TestSameTableNoJoinNotNull()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSameTableNoJoinNotNull(db);
        ExecSql(db);
      }
    }

    private void RunSameTableNoJoinNotNull(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .Where.Add("name", "Freeve");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal(1, shops.Count);

        Assert.Equal("/freeve/main.jpq", shops[0].GetRecord("main_image", conn).GetString("path"));
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Equal("/freeve/sub.jpq", shops[0].GetRecord("sub_image", conn).GetString("path"));
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
      }
    }

    [Fact]
    public void TestSameTableNoJoinNull()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSameTableNoJoinNull(db);
        ExecSql(db);
      }
    }

    private void RunSameTableNoJoinNull(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .Where.Add("name", "ビーナスラッシュ");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(1, shops.Count);

        Assert.Null(shops[0].GetRecord("main_image", conn));
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        Assert.Null(shops[0].GetRecord("sub_image", conn));
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
      }
    }

    [Fact]
    public void TestRecordCache()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunRecordCache(db);
        ExecSql(db);
      }
    }

    private void RunRecordCache(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .Where.Add("name", "天府舫");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord(select);
        Assert.True(shop is Test.Orm.Shop);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        var area = shop.GetRecord("area", conn);
        Assert.True(area is Test.Orm.Area);
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);

        //キャッシュされている（Connection必要なし）
        var area2 = shop.GetRecord("area");
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.Equal(area, area2);

        shop.ClearRecordCache();

        var area3 = shop.GetRecord("area");
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.NotEqual(area2, area3);
      }
    }

    [Fact]
    public void TestRecordNoTable()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunRecordNoTable(db);
        ExecSql(db);
      }
    }

    private void RunRecordNoTable(TestDb db)
    {
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom("shop")
         .AddColumn("id", "name")
         .Where.Add("name", "天府舫");

      select.Context("shop").InnerJoin(
        "menu",
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("id", "shop"),
          new Sdx.Db.Sql.Column("shop_id", "menu")
        )
      );

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        //Tableを使ってないと、MetaDataが取れないので主キーがわからず組み立てられない。
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          var shop = conn.FetchRecord(select);
        }));

        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("Use Sdx.Db.Table, if you want to get Record.", ex.Message);
      }
    }

    [Fact]
    public void TestChangeColumn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunChangeColumn(db);
        ExecSql(db);
      }
    }

    private void RunChangeColumn(TestDb db)
    {
      var select = db.Adapter.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop())

        .ClearColumns()
        .Table.AddColumns("id", "name", "main_image_id")
        .Context.AddOrder("id", Sdx.Db.Sql.Order.ASC).Table.Select
        .SetLimit(2);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);

        Assert.Equal("天祥", shops[0].GetString("name"));
        Assert.Equal("エスペリア", shops[1].GetString("name"));

        //取得しなかったキーを取得すると例外
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          var areaEx = shops[0].GetValue("area_id");
        }));

        Assert.IsType<InvalidOperationException>(ex);

        Assert.False(shops[0].CanGetValue("area_id"));
      }
    }

    [Fact]
    public void TestGetter()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGetter(db);
        ExecSql(db);
      }
    }

    private void RunGetter(TestDb db)
    {
      var select = db.Adapter.CreateSelect();

      select
        .AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Sql.Order.ASC);
      select
        .SetLimit(1);
      ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord(select);

        var datetime = shop.GetDateTime("created_at");
        Assert.IsType<DateTime>(datetime);
        Assert.Equal("2015/01/01 12:30:00", datetime.ToString("yyyy/MM/dd HH:mm:ss"));

        var dec = shop.GetDecimal("id");
        Assert.IsType<decimal>(dec);
        Assert.Equal(1.0m, dec);

        var dbl = shop.GetDouble("id");
        Assert.IsType<double>(dbl);
        Assert.Equal(1.0, dbl);

        var shr = shop.GetInt16("id");
        Assert.IsType<short>(shr);
        Assert.Equal(1, shr);

        var it = shop.GetInt32("id");
        Assert.IsType<int>(it);
        Assert.Equal(1, it);

        var lng = shop.GetInt64("id");
        Assert.IsType<long>(lng);
        Assert.Equal(1, lng);

        var str = shop.GetString("id");
        Assert.IsType<string>(str);
        Assert.Equal("1", str);
      }
    }

    [Fact]
    public void TestWithNullValue()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RutWithNullValue(db);
        ExecSql(db);
      }
    }

    private void RutWithNullValue(TestDb testDb)
    {
      var db = testDb.Adapter;

      Sdx.Db.Record shop = new Test.Orm.Shop();
      shop.SetValue("name", "RecordNullValueTest");
      shop.SetValue("area_id", "1");
      shop.SetValue("login_id", "foobar");

      object id = null;
      using(var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        try
        {
          shop.Save(conn);
          conn.Commit();
          id = shop.GetValue("id");
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        Assert.Equal("foobar", shop.GetValue("login_id"));

        //保存後にSetしていない値を読むとDBNull.Value。
        Assert.Equal(DBNull.Value, shop.GetValue("password"));
        //DBNullでもCanGetValueはtrue
        Assert.True(shop.CanGetValue("password"));
      }
      
      //DbNullで更新
      using(var conn = db.CreateConnection())
      {
        conn.Open();
        var select = db.CreateSelect();
        select
          .AddFrom(new Test.Orm.Table.Shop())
          .WhereCall((where) => where.Add("id", id));

        select.Context("shop");

        shop = conn.FetchRecord(select);
        Assert.Equal(DBNull.Value, shop.GetValue("password"));

        shop.SetValue("login_id", DBNull.Value);
        conn.BeginTransaction();
        try
        {
          shop.Save(conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //再び取得
        shop = conn.FetchRecord(select);
        Assert.Equal(DBNull.Value, shop.GetValue("login_id"));
        Assert.True(shop.IsNull("login_id"));
      }


      //空文字で更新
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var select = db.CreateSelect();
        select
          .AddFrom(new Test.Orm.Table.Shop())
          .WhereCall((where) => where.Add("id", id));

        shop = conn.FetchRecord(select);
        shop.SetValue("login_id", "");
        conn.BeginTransaction();
        try
        {
          shop.Save(conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //再び取得
        shop = conn.FetchRecord(select);
        Assert.Equal(DBNull.Value, shop.GetValue("login_id"));
        Assert.True(shop.IsNull("login_id"));

        //空文字の挿入は
        shop.SetValue("login_id", "", true);
        conn.BeginTransaction();
        try
        {
          shop.Save(conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        shop = conn.FetchRecord(select);
        Assert.Equal("", shop.GetValue("login_id"));
        Assert.False(shop.IsNull("login_id"));
      }
    }

    [Fact]
    public void TestSetValues()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RutSetValues(db);
        ExecSql(db);
      }
    }

    private void RutSetValues(TestDb testDb)
    {
      var db = testDb.Adapter;

      Sdx.Db.Record shop = new Test.Orm.Shop();
      var values = new NameValueCollection();

      values.Add("name", "TestSetValues");
      values.Add("area_id", "1");
      values.Add("login_id", "TestSetValues_login_id");
      values.Add("foobar", "bazzz");

      shop.SetValues(values);

      using(var conn = db.CreateConnection())
      {
        conn.Open();
        conn.BeginTransaction();
        try
        {
          shop.Save(conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        shop = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, shop.GetString("id"));
        Assert.Equal("TestSetValues_login_id", shop.GetValue("login_id"));

        values = new NameValueCollection();
        values.Add("name", "TestSetValues");
        values.Add("area_id", "1");
        values.Add("login_id", "");

        shop.SetValues(values);

        conn.BeginTransaction();
        try
        {
          shop.Save(conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        shop = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, shop.GetString("id"));
        Assert.Equal(DBNull.Value, shop.GetValue("login_id"));
      }
    }

    /// <summary>
    /// このテストは特にアサートしてませんがドキュメント代わりにもなるしとっておきます。
    /// </summary>
    [Fact]
    public void TestCastMethods()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunCastMethods(db);
        ExecSql(db);
      }
    }

    private void RunCastMethods(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         ;

      select.Context("shop")
         .InnerJoin(new Test.Orm.Table.Menu())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC)
         ;

      select.Context("shop")
          .Where.Add("name", "天府舫");

      using(var conn = db.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);

        //foreachはキャストの必要がありません。
        foreach(Test.Orm.Shop shop in shops)
        {
          
        }

        //ForEachメソッドにはキャストメソッドがります。
        shops.ForEach<Test.Orm.Shop>(shop => {
          
        });

        //RecordSetはIEnumerableを実装しています。
        shops.Cast<Test.Orm.Shop>().Where(shop => shop.IsUpdated);

        //indexを指定して取得するジェネリックメソッド
        Assert.Equal("天府舫", shops.Get<Test.Orm.Shop>(0).GetString("name"));
      }
    }

    [Fact]
    public void TestSortRecordSet()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSortRecordSet(db);
        ExecSql(db);
      }
    }

    private void RunSortRecordSet(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      select.SetLimit(6);
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);

        shops[0].SetValue("area_id", 2);
        shops[1].SetValue("area_id", 3);
        shops[2].SetValue("area_id", 1);
        shops[3].SetValue("area_id", 4);
        shops[4].SetValue("area_id", 4);
        shops[5].SetValue("area_id", 5);

        Assert.Equal(2, shops[0].GetInt32("area_id"));
        Assert.Equal(3, shops[1].GetInt32("area_id"));
        Assert.Equal(1, shops[2].GetInt32("area_id"));
        Assert.Equal(4, shops[3].GetInt32("area_id"));
        Assert.Equal(4, shops[4].GetInt32("area_id"));
        Assert.Equal(5, shops[5].GetInt32("area_id"));


        shops.Sort((rec1, rec2) => rec1.GetInt32("area_id") - rec2.GetInt32("area_id"));

        Assert.Equal(1, shops[0].GetInt32("area_id"));
        Assert.Equal(2, shops[1].GetInt32("area_id"));
        Assert.Equal(3, shops[2].GetInt32("area_id"));
        Assert.Equal(4, shops[3].GetInt32("area_id"));
        Assert.Equal(4, shops[4].GetInt32("area_id"));
        Assert.Equal(5, shops[5].GetInt32("area_id"));

        shops.Sort((rec1, rec2) => rec2.GetInt32("area_id") - rec1.GetInt32("area_id"));

        Assert.Equal(5, shops[0].GetInt32("area_id"));
        Assert.Equal(4, shops[1].GetInt32("area_id"));
        Assert.Equal(4, shops[2].GetInt32("area_id"));
        Assert.Equal(3, shops[3].GetInt32("area_id"));
        Assert.Equal(2, shops[4].GetInt32("area_id"));
        Assert.Equal(1, shops[5].GetInt32("area_id"));
      }
    }

    [Fact]
    public void TestGroupingRecord()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGroupingRecord(db);
        ExecSql(db);
      }
    }

    private void RunGroupingRecord(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      select.SetLimit(6);
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);

        //INT
        shops[0].SetValue("area_id", 1);
        shops[1].SetValue("area_id", 1);
        shops[2].SetValue("area_id", 2);
        shops[3].SetValue("area_id", 2);
        shops[4].SetValue("area_id", 3);
        shops[5].SetValue("area_id", 3);

        var shops3 = shops.GroupByColumn("area_id", 3);
        Assert.IsType<Sdx.Db.RecordSet>(shops3);
        Assert.Equal(2, shops3.Count);
        Assert.Equal(3, shops3[0].GetInt32("area_id"));
        Assert.Equal(3, shops3[1].GetInt32("area_id"));

        var shops2 = shops.GroupByColumn("area_id", 2);
        Assert.IsType<Sdx.Db.RecordSet>(shops2);
        Assert.Equal(2, shops2.Count);
        Assert.Equal(2, shops2[0].GetInt32("area_id"));
        Assert.Equal(2, shops2[1].GetInt32("area_id"));

        var shops1 = shops.GroupByColumn("area_id", 1);
        Assert.IsType<Sdx.Db.RecordSet>(shops2);
        Assert.Equal(2, shops1.Count);
        Assert.Equal(1, shops1[0].GetInt32("area_id"));
        Assert.Equal(1, shops1[1].GetInt32("area_id"));

        shops[3].SetValue("area_id", 3);
        shops3 = shops.GroupByColumn("area_id", 3);
        //このメソッドはキャッシュするので変わらない。
        Assert.Equal(2, shops3.Count);

        shops.ClearGroupByColumnCache("area_id");
        shops3 = shops.GroupByColumn("area_id", 3);
        Assert.Equal(3, shops3.Count);



        //String
        shops[0].SetValue("area_id", "1");
        shops[1].SetValue("area_id", "1");
        shops[2].SetValue("area_id", "2");
        shops[3].SetValue("area_id", "2");
        shops[4].SetValue("area_id", "3");
        shops[5].SetValue("area_id", "3");

        shops3 = shops.GroupByColumn("area_id", "3");
        Assert.IsType<Sdx.Db.RecordSet>(shops3);
        Assert.Equal(2, shops3.Count);
        Assert.Equal("3", shops3[0].GetString("area_id"));
        Assert.Equal("3", shops3[1].GetString("area_id"));

        shops2 = shops.GroupByColumn("area_id", "2");
        Assert.IsType<Sdx.Db.RecordSet>(shops2);
        Assert.Equal(2, shops2.Count);
        Assert.Equal("2", shops2[0].GetString("area_id"));
        Assert.Equal("2", shops2[1].GetString("area_id"));

        shops1 = shops.GroupByColumn("area_id", "1");
        Assert.IsType<Sdx.Db.RecordSet>(shops2);
        Assert.Equal(2, shops1.Count);
        Assert.Equal("1", shops1[0].GetString("area_id"));
        Assert.Equal("1", shops1[1].GetString("area_id"));

        shops[3].SetValue("area_id", "3");
        shops3 = shops.GroupByColumn("area_id", "3");
        //このメソッドはキャッシュするので変わらない。
        Assert.Equal(2, shops3.Count);

        shops.ClearGroupByColumnCache("area_id");
        shops3 = shops.GroupByColumn("area_id", "3");
        Assert.Equal(3, shops3.Count);
      }
    }

    [Fact]
    public void TestPopByCount()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunPopByCount(db);
        ExecSql(db);
      }
    }

    private void RunPopByCount(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      select.SetLimit(6);
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet(select);
        Assert.Equal(6, shops.Count);
        Assert.Equal(1, shops[0].GetInt32("id"));
        Assert.Equal(2, shops[1].GetInt32("id"));
        Assert.Equal(3, shops[2].GetInt32("id"));
        Assert.Equal(4, shops[3].GetInt32("id"));
        Assert.Equal(5, shops[4].GetInt32("id"));
        Assert.Equal(6, shops[5].GetInt32("id"));


        var pops = shops.PopSet(2);
        Assert.Equal(2, pops.Count);
        Assert.Equal(1, pops[0].GetInt32("id"));
        Assert.Equal(2, pops[1].GetInt32("id"));
        Assert.Equal(4, shops.Count);
        Assert.Equal(3, shops[0].GetInt32("id"));
        Assert.Equal(4, shops[1].GetInt32("id"));
        Assert.Equal(5, shops[2].GetInt32("id"));
        Assert.Equal(6, shops[3].GetInt32("id"));

        pops = shops.PopSet(1);
        Assert.Equal(1, pops.Count);
        Assert.Equal(3, pops[0].GetInt32("id"));
        Assert.Equal(3, shops.Count);
        Assert.Equal(4, shops[0].GetInt32("id"));
        Assert.Equal(5, shops[1].GetInt32("id"));
        Assert.Equal(6, shops[2].GetInt32("id"));

        pops = shops.PopSet(0);
        Assert.Equal(0, pops.Count);
        Assert.Equal(3, shops.Count);
        Assert.Equal(4, shops[0].GetInt32("id"));
        Assert.Equal(5, shops[1].GetInt32("id"));
        Assert.Equal(6, shops[2].GetInt32("id"));

        pops = shops.PopSet(3);
        Assert.Equal(3, pops.Count);
        Assert.Equal(4, pops[0].GetInt32("id"));
        Assert.Equal(5, pops[1].GetInt32("id"));
        Assert.Equal(6, pops[2].GetInt32("id"));

        Assert.Equal(0, shops.Count);
      }
    }

    [Fact]
    public void TestShuffle()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunShuffle(db);
        ExecSql(db);
      }
    }

    private void RunShuffle(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      select.SetLimit(6);
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var tryCount = 10000;
        //直前の並び順と同じなった回数を数えて確率でAssertします。
        var sameCount = 0;
        var shops = conn.FetchRecordSet(select);
        string prev = shops.Select(rec => rec.GetString("id")).Aggregate((sum, val) => sum + val);
        for (int i = 0; i < tryCount; i++)
        {
          var shuffled = shops.Shuffle().Select(rec => rec.GetString("id")).Aggregate((sum, val) => sum + val);
          if(prev == shuffled)
          {
            sameCount++;
          }
          prev = shuffled;
        }

        //3~4%は前回と同じ並び順になってしまうので10%でチェックします。
        Assert.True(tryCount * 0.1 > sameCount);
      }
    }

    [Fact]
    public void TestExtraValue()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunExtraValue(db);
        ExecSql(db);
      }
    }

    private void RunExtraValue(TestDb testDb)
    {
      var db = testDb.Adapter;

      var select = db.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Shop(), cShop => 
      {
        cShop.Table.AddColumn(Sdx.Db.Sql.Expr.Wrap("CONCAT(name, '@', id)"), "extra_value");
      });

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        foreach(var shop in conn.FetchRecordSet(select))
        {
          var expect = string.Format("{0}@{1}", shop.GetString("name"), shop.GetString("id"));
          Assert.Equal(expect, shop.GetString("extra_value"));
        }
      }
    }
  }
}
