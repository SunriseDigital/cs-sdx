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
  public class DbRecordTest : BaseDbTest
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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(2, shops.Count);

        Assert.Equal("1", shops[0].GetString("id"));
        Assert.Equal("天祥", shops[0].GetString("name"));
        Assert.Equal("2", shops[0].GetString("area_id"));
        Assert.Equal("", shops[0].GetString("main_image_id"));
        Assert.Equal("", shops[0].GetString("sub_image_id"));

        var area = shops[0].GetRecordSet<Test.Orm.Area>("area")[0];
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal("新中野", area.GetString("name"));

        Assert.Equal("2", shops[1].GetString("id"));
        Assert.Equal("エスペリア", shops[1].GetString("name"));
        Assert.Equal("3", shops[1].GetString("area_id"));
        Assert.Equal("", shops[1].GetString("main_image_id"));
        Assert.Equal("", shops[1].GetString("sub_image_id"));

        area = shops[1].GetRecordSet<Test.Orm.Area>("area").First;
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(1, shops.Count);
        Assert.Equal("天府舫", shops[0].GetString("name"));

        var menuList = shops[0].GetRecordSet<Test.Orm.Menu>("menu");
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);
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
        .Where.Add("name", new List<string>() { "ビーナスラッシュ", "Freeve" }).Context
        .InnerJoin(new Test.Orm.Table.ShopCategory())
         ;

      select.Context("shop_category").InnerJoin(new Test.Orm.Table.Category())
        .AddOrder("id", Sdx.Db.Sql.Order.ASC);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(2, shops.Count);

        Assert.Equal("Freeve", shops[0].GetString("name"));
        Assert.Equal(3, shops[0].GetRecordSet<Test.Orm.ShopCategory>("shop_category").Count);
        Assert.Equal("美容室", shops[0].GetRecordSet<Test.Orm.ShopCategory>("shop_category")[0].GetRecordSet<Test.Orm.Category>("category").First.GetString("name"));
        Assert.Equal("ネイルサロン", shops[0].GetRecordSet<Test.Orm.ShopCategory>("shop_category")[1].GetRecordSet<Test.Orm.Category>("category").First.GetString("name"));
        Assert.Equal("まつげエクステ", shops[0].GetRecordSet<Test.Orm.ShopCategory>("shop_category")[2].GetRecordSet<Test.Orm.Category>("category").First.GetString("name"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal("ビーナスラッシュ", shops[1].GetString("name"));
        Assert.Equal(2, shops[1].GetRecordSet<Test.Orm.ShopCategory>("shop_category").Count);
        Assert.Equal("ネイルサロン", shops[0].GetRecordSet<Test.Orm.ShopCategory>("shop_category")[1].GetRecordSet<Test.Orm.Category>("category").First.GetString("name"));
        Assert.Equal("まつげエクステ", shops[0].GetRecordSet<Test.Orm.ShopCategory>("shop_category")[2].GetRecordSet<Test.Orm.Category>("category").First.GetString("name"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);
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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        var areaSet = shops[0].GetRecordSet<Test.Orm.Area>("area", conn);
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(1, areaSet.Count);
        Assert.Equal("新中野", areaSet[0].GetString("name"));

        var largeAreaSet = areaSet[0].GetRecordSet<Test.Orm.LargeArea>("large_area", conn);
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Queries.Count);

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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);
        var menuSet = shops[0].GetRecordSet<Test.Orm.Menu>(
          "menu",
          conn,
          sel => { sel.Context("menu").AddOrder("id", Sdx.Db.Sql.Order.ASC); }
        );
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(3, menuSet.Count);
        Assert.Equal("干し豆腐のサラダ", menuSet[0].GetString("name"));
        Assert.Equal("麻婆豆腐", menuSet[1].GetString("name"));
        Assert.Equal("牛肉の激辛水煮", menuSet[2].GetString("name"));

        //ORDERをひっくり返す
        menuSet = shops[0].ClearRecordCache("menu").GetRecordSet<Test.Orm.Menu>(
          "menu",
          conn,
          sel => { sel.Context("menu").AddOrder("id", Sdx.Db.Sql.Order.DESC); }
        );
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Queries.Count);

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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);
        var shopCategorySet = shops[0].GetRecordSet<Test.Orm.ShopCategory>(
          "shop_category",
          conn,
          sel => { sel.AddOrder("category_id", Sdx.Db.Sql.Order.ASC); }
        );
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(3, shopCategorySet.Count);
        Assert.Equal("美容室", shopCategorySet[0].GetRecordSet<Test.Orm.Category>("category", conn).First.GetString("name"));
        Assert.Equal("ネイルサロン", shopCategorySet[1].GetRecordSet<Test.Orm.Category>("category", conn).First.GetString("name"));
        Assert.Equal("まつげエクステ", shopCategorySet[2].GetRecordSet<Test.Orm.Category>("category", conn).First.GetString("name"));
        Assert.Equal(5, Sdx.Context.Current.DbProfiler.Queries.Count);

        shopCategorySet = shops[0].ClearRecordCache("shop_category").GetRecordSet<Test.Orm.ShopCategory>(
          "shop_category",
          conn,
          sel => { sel.AddOrder("category_id", Sdx.Db.Sql.Order.DESC); }
        );
        Assert.Equal(6, Sdx.Context.Current.DbProfiler.Queries.Count);


        Assert.Equal(3, shopCategorySet.Count);
        Assert.Equal("まつげエクステ", shopCategorySet[0].GetRecordSet<Test.Orm.Category>("category", conn).First.GetString("name"));
        Assert.Equal("ネイルサロン", shopCategorySet[1].GetRecordSet<Test.Orm.Category>("category", conn).First.GetString("name"));
        Assert.Equal("美容室", shopCategorySet[2].GetRecordSet<Test.Orm.Category>("category", conn).First.GetString("name"));
        Assert.Equal(9, Sdx.Context.Current.DbProfiler.Queries.Count);
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

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .LeftJoin(new Test.Orm.Table.Image(), "main_image").ParentContext
         .LeftJoin(new Test.Orm.Table.Image(), "sub_image").ParentContext
         .Where.Add("name", "Freeve");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(1, shops.Count);

        Assert.Equal("/freeve/main.jpq", shops[0].GetRecord<Test.Orm.Image>("main_image").GetString("path"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal("/freeve/sub.jpq", shops[0].GetRecord<Test.Orm.Image>("sub_image").GetString("path"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);
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

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .LeftJoin(new Test.Orm.Table.Image(), "main_image").ParentContext
         .LeftJoin(new Test.Orm.Table.Image(), "sub_image").ParentContext
         .Where.Add("name", "ビーナスラッシュ");

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, shops.Count);

        Assert.Null(shops[0].GetRecord<Test.Orm.Image>("main_image"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Null(shops[0].GetRecord<Test.Orm.Image>("sub_image"));
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);
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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal(1, shops.Count);

        Assert.Equal("/freeve/main.jpq", shops[0].GetRecord<Test.Orm.Image>("main_image", conn).GetString("path"));
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Equal("/freeve/sub.jpq", shops[0].GetRecord<Test.Orm.Image>("sub_image", conn).GetString("path"));
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Queries.Count);
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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);
        Assert.Equal(1, shops.Count);

        Assert.Null(shops[0].GetRecord<Test.Orm.Image>("main_image", conn));
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);

        Assert.Null(shops[0].GetRecord<Test.Orm.Image>("sub_image", conn));
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Queries.Count);
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
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);
        Assert.True(shop is Test.Orm.Shop);
        Assert.Equal(1, Sdx.Context.Current.DbProfiler.Queries.Count);

        var area = shop.GetRecord<Test.Orm.Area>("area", conn);
        Assert.True(area is Test.Orm.Area);
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);

        //キャッシュされている（Connection必要なし）
        var area2 = shop.GetRecord<Test.Orm.Area>("area");
        Assert.Equal(2, Sdx.Context.Current.DbProfiler.Queries.Count);
        Assert.Equal(area, area2);

        shop.ClearRecordCache();

        //キャッシュがクリアされたのでConnectionなしだと例外に
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          var areaEx = shop.GetRecord<Test.Orm.Area>("area");
        }));

        Assert.IsType<ArgumentNullException>(ex);

        var area3 = shop.GetRecord<Test.Orm.Area>("area", conn);
        Assert.Equal(3, Sdx.Context.Current.DbProfiler.Queries.Count);
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
        Test.Orm.Shop.Meta.CreateJoinCondition("menu")
      );

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        //Tableを使ってないと、MetaDataが取れないので主キーがわからず組み立てられない。
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          var shop = conn.FetchRecord<Test.Orm.Shop>(select);
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
        var shops = conn.FetchRecordSet<Test.Orm.Shop>(select);

        Assert.Equal("天祥", shops[0].GetString("name"));
        Assert.Equal("エスペリア", shops[1].GetString("name"));

        //取得しなかったキーは取得を試みると例外になる。
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
          shops[0].GetString("area_id");
        }));

        Assert.IsType<KeyNotFoundException>(ex);
        Assert.Equal("Missing area_id@shop key.", ex.Message);
        ;
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
        .AddOrder("id", Sdx.Db.Sql.Order.ASC).Select
        .SetLimit(1);
      ;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord<Test.Orm.Shop>(select);

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
  }
}
