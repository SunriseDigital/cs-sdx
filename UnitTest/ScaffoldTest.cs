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
using Moq;
using System.Collections.Specialized;
using System.Web;
using System.IO;

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

    [Fact]
    public void TestGrouping()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGrouping(db);
        ExecSql(db);
      }
    }

    private void RunGrouping(TestDb db)
    {
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=1"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() => 
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name");

        scaffold.InitGroup();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=1", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=1", scaffold.ListPageUrl.Build());

        var actualSet = scaffold.FetchRecordSet();
        actualSet.ForEach((rec) =>
        {
          Assert.Equal("1", rec.GetString("large_area_id"));
        });      
      }))();


      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=2"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name");

        scaffold.InitGroup();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=2", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=2", scaffold.ListPageUrl.Build());

        var actualSet = scaffold.FetchRecordSet();
        actualSet.ForEach((rec) =>
        {
          Assert.Equal("2", rec.GetString("large_area_id"));
        });
      }))();

    }
  }
}
