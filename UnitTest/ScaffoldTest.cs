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
using System.Collections.Generic;

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
    public void TestGroupingTableMeta()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGroupingTableMeta(db);
        ExecSql(db);
      }
    }

    private void RunGroupingTableMeta(TestDb db)
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
        Assert.Equal("東京", scaffold.Group.Name);

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
        Assert.Equal("愛知", scaffold.Group.Name);

        var actualSet = scaffold.FetchRecordSet();
        actualSet.ForEach((rec) =>
        {
          Assert.Equal("2", rec.GetString("large_area_id"));
        });

        Assert.False(scaffold.Group.HasSelector);

      }))();

      //GroupSelector
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=1"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name", "SelectDefaultOrder");

        scaffold.InitGroup();

        Assert.True(scaffold.Group.HasSelector);

        var select = scaffold.Group.BuildSelector();
        Assert.IsType<Sdx.Html.Select>(select);
        Assert.Equal("<select name=\"large_area_id\"><option value=\"\">全て</option><option value=\"1\" selected>東京</option><option value=\"2\">愛知</option></select>", select.Tag.Render());

        List<Sdx.Html.Option> options = (List<Sdx.Html.Option>)select.Options;
        Assert.Equal("", options[0].Value.ToString());//最初は空

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var actual = conn.FetchRecordSet(Test.Orm.Table.LargeArea.Meta, (sel) => sel.Context("large_area").Table.SelectDefaultOrder(sel));
          var key = 1;
          actual.ForEach((rec) =>
          {
            Assert.Equal(rec.GetString("id"), options[key].Tag.Attr["value"]);
            ++key;
          });
        }

      }))();

      //Group strict without handler
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", ""),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name");
        scaffold.Group.Strict = true;

        //Handlerが無ければ例外
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          scaffold.InitGroup();
        }));

        Assert.IsType<HttpException>(ex);
      }))();

      //Group strict with handler
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", ""),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name");
        scaffold.Group.Strict = true;

        var handlerCalled = false;
        Sdx.Context.Current.HttpErrorHandler.SetHandler(404, () => handlerCalled = true);
        scaffold.InitGroup();
        Assert.True(handlerCalled);

      }))();

      //Group strict selector
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=2"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, "name", "SelectDefaultOrder");
        scaffold.Group.Strict = true;

        var select = scaffold.InitGroup();
        Assert.Equal("<select name=\"large_area_id\"><option value=\"1\">東京</option><option value=\"2\" selected>愛知</option></select>", select.Tag.Render());

      }))();
    }

    [Fact]
    public void TestGroupingStaticClass()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGroupingStaticClass(db);
        ExecSql(db);
      }
    }

    private void RunGroupingStaticClass(TestDb db)
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

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), "GetName");

        scaffold.InitGroup();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=1", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=1", scaffold.ListPageUrl.Build());
        Assert.Equal("東京", scaffold.Group.Name);

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

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), "GetName");

        scaffold.InitGroup();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=2", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=2", scaffold.ListPageUrl.Build());
        Assert.Equal("愛知", scaffold.Group.Name);

        var actualSet = scaffold.FetchRecordSet();
        actualSet.ForEach((rec) =>
        {
          Assert.Equal("2", rec.GetString("large_area_id"));
        });
      }))();

      //GroupSelector
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=1"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), "GetName", "GetList");

        scaffold.InitGroup();

        Assert.True(scaffold.Group.HasSelector);

        var select = scaffold.Group.BuildSelector();
        Assert.IsType<Sdx.Html.Select>(select);
        Assert.Equal("<select name=\"large_area_id\"><option value=\"\">全て</option><option value=\"1\" selected>東京</option><option value=\"2\">愛知</option></select>", select.Tag.Render());

        List<Sdx.Html.Option> options = (List<Sdx.Html.Option>)select.Options;
        Assert.Equal("", options[0].Value.ToString());//最初は空

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var actual = conn.FetchRecordSet(Test.Orm.Table.LargeArea.Meta, (sel) => sel.Context("large_area").Table.SelectDefaultOrder(sel));
          var key = 1;
          actual.ForEach((rec) =>
          {
            Assert.Equal(rec.GetString("id"), options[key].Tag.Attr["value"]);
            ++key;
          });
        }

      }))();
    }
  }
}
