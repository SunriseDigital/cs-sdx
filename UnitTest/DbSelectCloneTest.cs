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

namespace UnitTest
{
  [TestClass]
  public class DbSelectCloneTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    private Sdx.Db.Sql.Select CreateCommonSelect(TestDb db)
    {
      var select = db.Adapter.CreateSelect();

      //FROM + JOIN
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .InnerJoin(new Test.Orm.Table.Menu());

      Sdx.Db.Sql.Select sub = db.Adapter.CreateSelect();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").InnerJoin(
        sub,
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("area_id", "shop"),
          new Sdx.Db.Sql.Column("id", "sub_area")
        ),
        "sub_area"
      );

      select.Context("shop").InnerJoin(
        Sdx.Db.Sql.Expr.Wrap("(SELECT id FROM area WHERE id = 1)"),
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("area_id", "shop"),
          new Sdx.Db.Sql.Column("id", "sub_area_1")
        ),
        "sub_area_1"
      );

      select.Context("shop").InnerJoin(
        "image",
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("main_image_id", "shop"),
          new Sdx.Db.Sql.Column("id", "main_image")
        ),
        "main_image"
      );

      //Column
      select.Context("sub_area").AddColumn("id");
      select.Context("sub_area_1").AddColumn("id");
      select.Context("main_image").AddColumn("id");

      return select;
    }

    [Fact]
    public void TestContext()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunContext(db);
        ExecSql(db);
      }
    }

    private void RunContext(TestDb db)
    {
      var origin = this.CreateCommonSelect(db);
      var cloned = (Sdx.Db.Sql.Select)origin.Clone();

      foreach (var contextName in new String[] { "shop", "menu", "sub_area", "sub_area_1", "main_image" })
      {
        Assert.NotEqual(
          origin.Context(contextName),
          cloned.Context(contextName)
        );

        if (origin.Context(contextName).Target is Sdx.Db.Sql.Select)
        {
          Assert.NotEqual(
            origin.Context(contextName).Target,
            cloned.Context(contextName).Target
          );
        }
        else
        {
          //Expr/Stringはimmutableなのでコピーしない
          Assert.Equal(
            origin.Context(contextName).Target,
            cloned.Context(contextName).Target
          );
        }

        Assert.Equal(
          origin,
          origin.Context(contextName).Select
        );

        Assert.Equal(
          cloned,
          cloned.Context(contextName).Select
        );

        if (origin.Context(contextName).Table != null)
        {
          Assert.NotEqual(
            origin.Context(contextName).Table,
            cloned.Context(contextName).Table
          );

          Assert.Equal(
            origin,
            origin.Context(contextName).Table.Select
          );

          Assert.Equal(
            cloned,
            cloned.Context(contextName).Table.Select
          );
        }
      }
    }

    [Fact]
    public void TestColumn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunColumn(db);
        ExecSql(db);
      }
    }

    private void RunColumn(TestDb db)
    {
      var origin = this.CreateCommonSelect(db);
      var cloned = (Sdx.Db.Sql.Select)origin.Clone();

      cloned.Context("main_image").ClearColumns();

      Assert.NotEqual(origin.Build().CommandText, cloned.Build().CommandText);
    }

    [Fact]
    public void TestGroup()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGroup(db);
        ExecSql(db);
      }
    }

    private void RunGroup(TestDb db)
    {
      var origin = this.CreateCommonSelect(db);
      var cloned = (Sdx.Db.Sql.Select)origin.Clone();

      cloned.Context("shop").AddGroup("id");

      Assert.NotEqual(origin.Build().CommandText, cloned.Build().CommandText);
    }

    [Fact]
    public void TestOrder()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunOrder(db);
        ExecSql(db);
      }
    }

    private void RunOrder(TestDb db)
    {
      var origin = this.CreateCommonSelect(db);
      var cloned = (Sdx.Db.Sql.Select)origin.Clone();

      cloned.Context("shop").AddOrder("id", Sdx.Db.Sql.Order.ASC);

      Assert.NotEqual(origin.Build().CommandText, cloned.Build().CommandText);
    }


    [Fact]
    public void TestWhere()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunWhere(db);
        ExecSql(db);
      }
    }

    private void RunWhere(TestDb db)
    {
      var origin = this.CreateCommonSelect(db);
      var cloned = (Sdx.Db.Sql.Select)origin.Clone();

      cloned.Where.Add("id", "1");

      Assert.NotEqual(origin.Build().CommandText, cloned.Build().CommandText);
    }

    [Fact]
    public void TestHaving()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunHaving(db);
        ExecSql(db);
      }
    }

    private void RunHaving(TestDb db)
    {
      var origin = this.CreateCommonSelect(db);
      var cloned = (Sdx.Db.Sql.Select)origin.Clone();

      cloned.Having.Add(Sdx.Db.Sql.Expr.Wrap("shop.id"), "1");

      Assert.NotEqual(origin.Build().CommandText, cloned.Build().CommandText);
    }



  }
}
