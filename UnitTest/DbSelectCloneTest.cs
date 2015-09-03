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

    [Fact]
    public void TestContext()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFromTable(db);
        ExecSql(db);
      }
    }

    private void RunFromTable(TestDb db)
    {
      var origin = new Sdx.Db.Query.Select();

      origin
        .AddFrom(new Test.Orm.Table.Shop())
        .InnerJoin(new Test.Orm.Table.Menu());

      Sdx.Db.Query.Select sub = new Sdx.Db.Query.Select();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      origin.Context("shop").InnerJoin(
        sub,
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Query.Column("area_id", "shop"),
          new Sdx.Db.Query.Column("id", "sub_cat")
        ),
        "sub_cat"
      );

      var cloned = (Sdx.Db.Query.Select)origin.Clone();

      foreach (var contextName in new String[]{ "shop", "menu", "sub_cat" })
      {
        Assert.NotEqual(
          origin.Context(contextName),
          cloned.Context(contextName)
        );

        if(origin.Context(contextName).Target is Sdx.Db.Query.Select)
        {
          Assert.NotEqual(
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

        if(origin.Context(contextName).Table != null)
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
  }
}
