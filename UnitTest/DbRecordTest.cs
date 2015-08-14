﻿using Xunit;
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
         .AddOrder("id", Sdx.Db.Query.Order.ASC);
      select.Limit = 1;

      Sdx.Db.Query.Result result = select.Execute();
      var shops = result.Group("shop");
      shops.ForEach(shop => {
        Assert.Equal("1", shop.GetString("id"));
        Assert.Equal("天祥", shop.GetString("name"));
        Assert.Equal("2", shop.GetString("area_id"));
        Assert.Equal("", shop.GetString("main_image_id"));
        Assert.Equal("", shop.GetString("sub_image_id"));
      });
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
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Query.Order.ASC)
         .InnerJoin(new Test.Orm.Table.Area())
         ;
      select.Limit = 2;

      Sdx.Db.Query.Result result = select.Execute();
      var shops = result.Group("shop");
      Assert.Equal(2, shops.Count);

      Assert.Equal("1", shops[0].GetString("id"));
      Assert.Equal("天祥", shops[0].GetString("name"));
      Assert.Equal("2", shops[0].GetString("area_id"));
      Assert.Equal("", shops[0].GetString("main_image_id"));
      Assert.Equal("", shops[0].GetString("sub_image_id"));

      var area = shops[0].Group("area")[0];
      Assert.Equal("新中野", area.GetString("name"));

      Assert.Equal("2", shops[1].GetString("id"));
      Assert.Equal("エスペリア", shops[1].GetString("name"));
      Assert.Equal("3", shops[1].GetString("area_id"));
      Assert.Equal("", shops[1].GetString("main_image_id"));
      Assert.Equal("", shops[1].GetString("sub_image_id"));

      area = shops[1].Group("area").First;
      Assert.Equal("西麻布", area.GetString("name"));
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
      var select = db.Adapter.CreateSelect();

      select
         .AddFrom(new Test.Orm.Table.Shop())
         .AddOrder("id", Sdx.Db.Query.Order.ASC)
         ;

      select.Context("shop")
         .InnerJoin(new Test.Orm.Table.Menu())
         .AddOrder("id", Sdx.Db.Query.Order.ASC)
         ;

      select.Context("shop")
          .Where.Add("name", "天府舫");

      var result = select.Execute();
      var shops = result.Group("shop");

      Assert.Equal(1, shops.Count);
      Assert.Equal("天府舫", shops[0].GetString("name"));

      var menuList = shops[0].Group("menu");
      Assert.Equal(3, menuList.Count);
      Assert.Equal("干し豆腐のサラダ", menuList[0].GetString("name"));
      Assert.Equal("麻婆豆腐", menuList[1].GetString("name"));
      Assert.Equal("牛肉の激辛水煮", menuList[2].GetString("name"));
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
      var select = db.Adapter.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop())
        .AddOrder("id", Sdx.Db.Query.Order.ASC)
        .Where.Add("name", new List<string>() { "Terra Blue", "Freeve" })
         ;

      select.Context("shop").InnerJoin(new Test.Orm.Table.ShopCategory())
         ;

      select.Context("shop_category").InnerJoin(new Test.Orm.Table.Category())
        .AddOrder("id", Sdx.Db.Query.Order.ASC);

      var result = select.Execute();
      var shops = result.Group("shop");

      Assert.Equal(2, shops.Count);

      Assert.Equal("Freeve", shops[0].GetString("name"));
      Assert.Equal(3, shops[0].Group("shop_category").Count);
      Assert.Equal("美容室", shops[0].Group("shop_category")[0].Group("category").First.GetString("name"));
      Assert.Equal("ネイルサロン", shops[0].Group("shop_category")[1].Group("category").First.GetString("name"));
      Assert.Equal("まつげエクステ", shops[0].Group("shop_category")[2].Group("category").First.GetString("name"));

      Assert.Equal("Terra Blue", shops[1].GetString("name"));
      Assert.Equal(2, shops[1].Group("shop_category").Count);
      Assert.Equal("ネイルサロン", shops[0].Group("shop_category")[1].Group("category").First.GetString("name"));
      Assert.Equal("まつげエクステ", shops[0].Group("shop_category")[2].Group("category").First.GetString("name"));

    }
  }
}
