using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
  [TestClass]
  public class Db_Adapter : BaseTest
  {
    [ClassInitialize]
    public static void InitilizeClass(TestContext context)
    {
      Console.WriteLine("FixtureSetUp");
      //最初のテストメソッドを実行する前に一回だけ実行したい処理はここ
    }

    [ClassCleanup]
    public static void CleanupClass()
    {
      Console.WriteLine("FixtureTearDown");
      //全てのテストメソッドが実行された後一回だけ実行する処理はここ
    }



    override public void FixtureSetUp()
    {
      Db_Adapter.InitilizeClass(null);
      //ここのクラス名は適宜書き換えてください。
      //MSTestのFixtureSetUpがstaticじゃないとだめだったのでこのような構造になってます。
    }

    override public void FixtureTearDown()
    {
      Db_Adapter.CleanupClass();
      //@see FixtureSetUp
    }


    [Fact]
    public void TestManagerOnlyRead()
    {
      var main = new Sdx.Db.Adapter.Manager();

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer read " + i.ToString();
        main.AddReadAdapter(db);
      }

      var connStr = new List<string>();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Read;
        Assert.True(db.ConnectionString.StartsWith("SqlServer read"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(5, connStr.Count);

      Exception ex2 = Record.Exception(new Assert.ThrowsDelegate(() =>
      {
        var db = main.Write;
      }));

      Assert.IsType<InvalidOperationException>(ex2);
    }

    [Fact]
    public void TestManagerOnlyWrite()
    {
      var main = new Sdx.Db.Adapter.Manager();

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer write " + i.ToString();
        main.AddWriteAdapter(db);
      }

      var connStr = new List<string>();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Write;
        Assert.True(db.ConnectionString.StartsWith("SqlServer write"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(5, connStr.Count);

      Exception ex2 = Record.Exception(new Assert.ThrowsDelegate(() =>
      {
        var db = main.Read;
      }));

      Assert.IsType<InvalidOperationException>(ex2);
    }

    [Fact]
    public void TestManagerBothOneWrite()
    {
      var main = new Sdx.Db.Adapter.Manager();

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer read " + i.ToString();
        main.AddReadAdapter(db);
      }

      for (int i = 0; i < 1; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer write " + i.ToString();
        main.AddWriteAdapter(db);
      }
      

      var connStr = new List<string>();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Read;
        Assert.True(db.ConnectionString.StartsWith("SqlServer read"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(5, connStr.Count);

      connStr.Clear();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Write;
        Assert.True(db.ConnectionString.StartsWith("SqlServer write"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(1, connStr.Count);
    }

    [Fact]
    public void TestManagerBothMutipleWrite()
    {
      var main = new Sdx.Db.Adapter.Manager();

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer read " + i.ToString();
        main.AddReadAdapter(db);
      }

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer write " + i.ToString();
        main.AddWriteAdapter(db);
      }


      var connStr = new List<string>();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Read;
        Assert.True(db.ConnectionString.StartsWith("SqlServer read"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(5, connStr.Count);

      connStr.Clear();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Write;
        Assert.True(db.ConnectionString.StartsWith("SqlServer write"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(5, connStr.Count);
    }

    [Fact]
    public void TestManagerAddCommon()
    {
      var main = new Sdx.Db.Adapter.Manager();

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer read " + i.ToString();
        main.AddReadAdapter(db);
      }

      for (int i = 0; i < 5; i++)
      {
        var db = new Sdx.Db.Adapter.SqlServer();
        db.ConnectionString = "SqlServer write " + i.ToString();
        main.AddCommonAdapter(db);
      }


      var connStr = new List<string>();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Read;
        Assert.True(
          db.ConnectionString.StartsWith("SqlServer read")
          ||
          db.ConnectionString.StartsWith("SqlServer write")
        );
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(10, connStr.Count);

      connStr.Clear();
      for (int i = 0; i < 300; i++)
      {
        var db = main.Write;
        Assert.True(db.ConnectionString.StartsWith("SqlServer write"));
        if (!connStr.Any(str => str == db.ConnectionString))
        {
          connStr.Add(db.ConnectionString);
        }
      }
      Assert.Equal(5, connStr.Count);
    }
  }
}
