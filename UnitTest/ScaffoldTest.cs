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
  public class ScaffoldTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    protected override void TearDown()
    {
      Sdx.Scaffold.Manager.ClearContextCache();
    }

    [Fact]
    public void TestSimpleList()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSimpleList(db);
        ExecSql(db);
      }
    }

    private void RunSimpleList(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter, db.Adapter.ToString());
      scaffold.DisplayList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
        );

      var actualSet = scaffold.FetchRecordSet();

      // build expected record set
      var select = db.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.LargeArea()).Table.SelectDefaultOrder(select);
      using(var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var expectedSet = conn.FetchRecordSet(select);

        Assert.Equal(expectedSet.Count, actualSet.Count);

        for(var i = 0; i < actualSet.Count; i++)
        {
          var aRecord = actualSet[i];
          var eRecord = expectedSet[i];
          foreach(var param in scaffold.DisplayList)
          {
            Assert.Equal(
              eRecord.GetString(param.Get("column")),
              aRecord.GetString(param.Get("column"))
            );
          }
        }
      }
    }


    [Fact]
    public void TestSimpleForm()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSimpleForm(db);
        ExecSql(db);
      }
    }

    private void RunSimpleForm(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter, db.Adapter.ToString());
      scaffold.FormList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "id")
          .Set("label", "ID")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
          .Set("label", "名称")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
          .Set("label", "コード")
        );

      var form = scaffold.BuildForm();
      Assert.IsType<Sdx.Html.InputHidden>(form["id"]);
      Assert.Equal("ID", form["id"].Label);
      Assert.Equal("id", form["id"].Tag.Attr["name"]);

      Assert.IsType<Sdx.Html.InputText>(form["name"]);
      Assert.Equal("名称", form["name"].Label);
      Assert.Equal("name", form["name"].Tag.Attr["name"]);

      Assert.IsType<Sdx.Html.InputText>(form["code"]);
      Assert.Equal("コード", form["code"].Label);
      Assert.Equal("code", form["code"].Tag.Attr["name"]);
    }
  }
}
