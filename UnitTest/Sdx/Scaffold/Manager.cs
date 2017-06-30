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
using System.Linq;
using System.Threading;

namespace UnitTest
{
  [TestClass]
  public class Scaffold_Manager : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    protected override void TearDown()
    {
      HttpContext.Current = null;
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter);
      scaffold.DisplayList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("code"))
        );

      // build expected record set
      var select = db.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.LargeArea()).Table.SelectDefaultOrder(select);
      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var actualSet = scaffold.FetchRecordSet(conn);
        var expectedSet = conn.FetchRecordSet(select);

        Assert.Equal(expectedSet.Count, actualSet.Count);

        for (var i = 0; i < actualSet.Count; i++)
        {
          var aRecord = actualSet[i];
          var eRecord = expectedSet[i];
          foreach (var param in scaffold.DisplayList)
          {
            Assert.Equal(
              eRecord.GetString(param["column"].ToString()),
              aRecord.GetString(param["column"].ToString())
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter);
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
          .Set("label", new Sdx.Scaffold.Config.Value("名称"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("code"))
          .Set("label", new Sdx.Scaffold.Config.Value("コード"))
        );

      //formの生成にレコードが必要なので接続を生成します。
      using (var conn = scaffold.Db.CreateConnection())
      {
        var record = scaffold.LoadRecord(new NameValueCollection(), conn);
        var form = scaffold.BuildForm(record, conn);

        Assert.IsType<Sdx.Html.InputText>(form["name"]);
        Assert.Equal("名称", form["name"].Label);
        Assert.Equal("name", form["name"].Tag.Attr["name"]);

        Assert.IsType<Sdx.Html.InputText>(form["code"]);
        Assert.Equal("コード", form["code"].Label);
        Assert.Equal("code", form["code"].Tag.Attr["name"]);
      }
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Sdx.Scaffold.Config.Value("name"));

        scaffold.Group.Init();

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();

          Assert.Null(scaffold.Group.BuildSelector(conn));
          Assert.Equal("/scaffold/area/edit.aspx?large_area_id=1", scaffold.EditPageUrl.Build());
          Assert.Equal("/scaffold/area/list.aspx?large_area_id=1", scaffold.ListPageUrl.Build());
          Assert.Equal("東京", scaffold.Group.Name);

          var actualSet = scaffold.FetchRecordSet(conn);
          actualSet.ForEach((rec) =>
          {
            Assert.Equal("1", rec.GetString("large_area_id"));
          });
        }

      }))();


      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=2"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Sdx.Scaffold.Config.Value("name"));

        scaffold.Group.Init();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=2", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=2", scaffold.ListPageUrl.Build());
        Assert.Equal("愛知", scaffold.Group.Name);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var actualSet = scaffold.FetchRecordSet(conn);
          actualSet.ForEach((rec) =>
          {
            Assert.Equal("2", rec.GetString("large_area_id"));
          });
        }


        Assert.False(scaffold.Group.HasSelector);

      }))();

      //GroupSelector
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=1"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Sdx.Scaffold.Config.Value("name"), new Sdx.Scaffold.Config.Value("FetchPairsForOption"));

        scaffold.Group.Init();

        Assert.True(scaffold.Group.HasSelector);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();


          var select = scaffold.Group.BuildSelector(conn);
          Assert.IsType<Sdx.Html.Select>(select);
          Assert.Equal("<select name=\"large_area_id\"><option value=\"\">絞り込む</option><option value=\"1\" selected>東京</option><option value=\"2\">愛知</option></select>", select.Tag.Render());

          List<Sdx.Html.Option> options = (List<Sdx.Html.Option>)select.Options;
          Assert.Equal("", options[0].Value.ToString());//最初は空

          var actual = Test.Orm.Table.LargeArea.Meta.CreateTable().FetchRecordSet(conn, (sel) => sel.Context("large_area").Table.SelectDefaultOrder(sel));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Sdx.Scaffold.Config.Value("name"));
        scaffold.Group.Strict = true;

        //Handlerが無ければ例外
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          scaffold.Group.Init();
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Sdx.Scaffold.Config.Value("name"));
        scaffold.Group.Strict = true;

        var handlerCalled = false;
        Sdx.Context.Current.HttpErrorHandler.SetHandler(404, () => handlerCalled = true);
        scaffold.Group.Init();
        Assert.True(handlerCalled);

      }))();

      //Group strict selector
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=2"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Sdx.Scaffold.Config.Value("name"), new Sdx.Scaffold.Config.Value("FetchPairsForOption"));
        scaffold.Group.Strict = true;

        scaffold.Group.Init();

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var select = scaffold.Group.BuildSelector(conn);
          Assert.Equal("<select name=\"large_area_id\"><option value=\"1\">東京</option><option value=\"2\" selected>愛知</option></select>", select.Tag.Render());
        }

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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"));

        scaffold.Group.Init();

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();

          Assert.Null(scaffold.Group.BuildSelector(conn));
          Assert.Equal("/scaffold/area/edit.aspx?large_area_id=1", scaffold.EditPageUrl.Build());
          Assert.Equal("/scaffold/area/list.aspx?large_area_id=1", scaffold.ListPageUrl.Build());
          Assert.Equal("東京", scaffold.Group.Name);

          var actualSet = scaffold.FetchRecordSet(conn);
          actualSet.ForEach((rec) =>
          {
            Assert.Equal("1", rec.GetString("large_area_id"));
          });
        }

      }))();


      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=2"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"));

        scaffold.Group.Init();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=2", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=2", scaffold.ListPageUrl.Build());
        Assert.Equal("愛知", scaffold.Group.Name);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var actualSet = scaffold.FetchRecordSet(conn);
          actualSet.ForEach((rec) =>
          {
            Assert.Equal("2", rec.GetString("large_area_id"));
          });
        }

      }))();

      //GroupSelector
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", "large_area_id=1"),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"), new Sdx.Scaffold.Config.Value("GetList"));

        scaffold.Group.Init();

        Assert.True(scaffold.Group.HasSelector);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();

          var select = scaffold.Group.BuildSelector(conn);
          Assert.IsType<Sdx.Html.Select>(select);
          Assert.Equal("<select name=\"large_area_id\"><option value=\"\">絞り込む</option><option value=\"1\" selected>東京</option><option value=\"2\">愛知</option></select>", select.Tag.Render());

          List<Sdx.Html.Option> options = (List<Sdx.Html.Option>)select.Options;
          Assert.Equal("", options[0].Value.ToString());//最初は空

          var actual = Test.Orm.Table.LargeArea.Meta.CreateTable().FetchRecordSet(conn, (sel) => sel.Context("large_area").Table.SelectDefaultOrder(sel));
          var key = 1;
          actual.ForEach((rec) =>
          {
            Assert.Equal(rec.GetString("id"), options[key].Tag.Attr["value"]);
            ++key;
          });
        }

      }))();
    }

    [Fact]
    public void TestGroupingFixedValue()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGroupingFixedValue(db);
        ExecSql(db);
      }
    }

    private void RunGroupingFixedValue(TestDb db)
    {
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", ""),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"));
        scaffold.Group.FixedValue = "1";
        scaffold.Group.Init();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=1", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=1", scaffold.ListPageUrl.Build());
        Assert.Equal("東京", scaffold.Group.Name);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var actualSet = scaffold.FetchRecordSet(conn);
          actualSet.ForEach((rec) =>
          {
            Assert.Equal("1", rec.GetString("large_area_id"));
          });
        }

      }))();

      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", ""),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"));
        scaffold.Group.FixedValue = "2";
        scaffold.Group.Init();

        Assert.Equal("/scaffold/area/edit.aspx?large_area_id=2", scaffold.EditPageUrl.Build());
        Assert.Equal("/scaffold/area/list.aspx?large_area_id=2", scaffold.ListPageUrl.Build());
        Assert.Equal("愛知", scaffold.Group.Name);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();
          var actualSet = scaffold.FetchRecordSet(conn);
          actualSet.ForEach((rec) =>
          {
            Assert.Equal("2", rec.GetString("large_area_id"));
          });
        }
      }))();

      //例外1
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", ""),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"), new Sdx.Scaffold.Config.Value("GetList"));
        scaffold.Group.FixedValue = "1";
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          scaffold.Group.Init();
        }));

        Assert.IsType<InvalidOperationException>(ex);
      }))();

      //例外2
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", ""),
        new HttpResponse(new StringWriter())
      );

      ((Action)(() =>
      {
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Sdx.Scaffold.Config.Value("GetName"));
        scaffold.Group.FixedValue = "1";
        scaffold.Group.DefaultValue = "2";
        Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
        {
          scaffold.Group.Init();
        }));

        Assert.IsType<InvalidOperationException>(ex);
      }))();
    }

    [Fact]
    public void TestDynamicGetter()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunDynamicGetter(db);
        ExecSql(db);
      }
    }

    private void RunDynamicGetter(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
      scaffold.DisplayList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("大エリア名"))
          .Set("dynamic", new Sdx.Scaffold.Config.Value("@large_area.name"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 100px;"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("dynamic", new Sdx.Scaffold.Config.Value("@large_area.#GetCode"))
        );

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var records = scaffold.FetchRecordSet(conn);
        Assert.Equal("東京", scaffold.DisplayList[0].Display(records[0], conn));
        Assert.Equal("tokyo", scaffold.DisplayList[1].Display(records[0], conn));
        Assert.Equal("愛知", scaffold.DisplayList[0].Display(records.First(r => r.GetDynamic("large_area_id") == 2), conn));
        Assert.Equal("aichi", scaffold.DisplayList[1].Display(records.First(r => r.GetDynamic("large_area_id") == 2), conn));
      }
    }

    [Fact]
    public void TestHtmlParams()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunHtmlParams(db);
        ExecSql(db);
      }
    }

    private void RunHtmlParams(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter);
      scaffold.DisplayList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("エリア編集"))
          .Set("html", new Sdx.Scaffold.Config.Value("<a href=\"/path/to/area/list?large_area_id={id}\">{name}</a>"))
          .Set("style", new Sdx.Scaffold.Config.Value("width: 100px;"))
        );

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var records = scaffold.FetchRecordSet(conn);
        Assert.Equal(
          "<a href=\"/path/to/area/list?large_area_id=1\">東京</a>",
          scaffold.DisplayList[0].Display(records[0], conn)
        );
        Assert.Equal(
          "<a href=\"/path/to/area/list?large_area_id=2\">愛知</a>",
          scaffold.DisplayList[0].Display(records[1], conn)
        );
      }

    }

    [Fact]
    public void TestManyManySave()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunManyManySave(db);
        ExecSql(db);
      }
    }

    private void RunManyManySave(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Shop.Meta, db.Adapter);
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("名前"))
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("場所"))
          .Set("column", new Sdx.Scaffold.Config.Value("area_id"))
        )
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("業種"))
          .Set("relation", new Sdx.Scaffold.Config.Value("shop_category"))
          .Set("column", new Sdx.Scaffold.Config.Value("category_id"))
        );

      var query = new NameValueCollection();

      var post = new NameValueCollection();
      post.Set("name", "foobar");
      post.Set("area_id", "1");
      post.Add("category_id", "1");
      post.Add("category_id", "2");

      string savedId;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        var record = scaffold.LoadRecord(query, conn);
        Assert.True(record.IsNew);

        var form = scaffold.BuildForm(record, conn);
        Assert.IsType<Sdx.Html.CheckBoxGroup>(form["category_id"]);

        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, post, conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //確認する
        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);
        Assert.Equal("foobar", savedRecord.GetString("name"));
        Assert.Equal("1", savedRecord.GetString("area_id"));

        var shopCategories = savedRecord.GetRecordSet("shop_category", conn, select => select.AddOrder("category_id", Sdx.Db.Sql.Order.ASC));
        Assert.Equal(2, shopCategories.Count);
        Assert.Equal("1", shopCategories[0].GetString("category_id"));
        Assert.Equal("2", shopCategories[1].GetString("category_id"));
      }

      //編集する
      query = new NameValueCollection();
      query.Set("id", savedId);

      post = new NameValueCollection();
      post.Set("name", "foobar");
      post.Set("area_id", "1");
      post.Add("category_id", "2");
      post.Add("category_id", "3");
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        var record = scaffold.LoadRecord(query, conn);
        Assert.False(record.IsNew);

        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, post, conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //確認する
        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);

        var shopCategories = savedRecord.GetRecordSet("shop_category", conn, select => select.AddOrder("category_id", Sdx.Db.Sql.Order.ASC));
        Assert.Equal(2, shopCategories.Count);
        Assert.Equal("2", shopCategories[0].GetString("category_id"));
        Assert.Equal("3", shopCategories[1].GetString("category_id"));
      }

      //削除する
      query = new NameValueCollection();
      query.Set("id", savedId);

      post = new NameValueCollection();
      post.Set("name", "foobar");
      post.Set("area_id", "1");
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        var record = scaffold.LoadRecord(query, conn);
        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, post, conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //確認する
        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);

        var shopCategories = savedRecord.GetRecordSet("shop_category", conn, select => select.AddOrder("category_id", Sdx.Db.Sql.Order.ASC));
        Assert.Equal(0, shopCategories.Count);
      }
    }

    [Fact]
    public void TestSwapRecordSetterMethod()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSwapRecordSetterMethod(db);
        ExecSql(db);
      }
    }

    private void RunSwapRecordSetterMethod(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("名前とコード"))
          .Set("name", new Sdx.Scaffold.Config.Value("name_with_code"))
          .Set("setter", new Sdx.Scaffold.Config.Value("SetNameWithCode"))//カンマ区切りの[名前,コード]をそれぞれnameとcodeにセットする。
        )
        ;

      var query = new NameValueCollection();
      var post = new NameValueCollection();
      post.Set("name_with_code", "名前,code");

      string savedId;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        var record = scaffold.LoadRecord(query, conn);
        var form = scaffold.BuildForm(record, conn);

        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, post, conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //確認する
        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Area()).FetchRecordByPkey(conn, savedId);
        Assert.Equal("名前", savedRecord.GetString("name"));
        Assert.Equal("code", savedRecord.GetString("code"));
      }
    }

    [Fact]
    public void TestSwapRecordAccessorUsingMethodInfo()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSwapRecordAccessorUsingMethodInfo(db);
        ExecSql(db);
      }
    }

    private void RunSwapRecordAccessorUsingMethodInfo(TestDb db)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter);
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("名前とコード"))
          .Set("column", new Sdx.Scaffold.Config.Value("name_with_code"))
          .Set("getter", new Sdx.Scaffold.Config.Value(typeof(Test.Orm.Area).GetMethod("GetNameWithCode")))
          .Set("setter", new Sdx.Scaffold.Config.Value(typeof(Test.Orm.Area).GetMethod("SetNameWithCode")))
        )
        ;

      ((Action)(() =>
      {
        var query = new NameValueCollection();
        var post = new NameValueCollection();
        post.Set("name_with_code", "名前,methodInfo");

        string savedId;
        using (var conn = scaffold.Db.CreateConnection())
        {
          conn.Open();

          var record = scaffold.LoadRecord(query, conn);
          var form = scaffold.BuildForm(record, conn);

          conn.BeginTransaction();
          try
          {
            scaffold.Save(record, post, conn);
            conn.Commit();
          }
          catch (Exception)
          {
            conn.Rollback();
            throw;
          }

          //確認する
          savedId = record.GetString("id");
          var savedRecord = (new Test.Orm.Table.Area()).FetchRecordByPkey(conn, savedId);
          Assert.Equal("名前", savedRecord.GetString("name"));
          Assert.Equal("methodInfo", savedRecord.GetString("code"));
        }
      }))();


      ((Action)(() =>
      {
        var query = new NameValueCollection();
        query.Add("id", "1");

        using (var conn = scaffold.Db.CreateConnection())
        {
          conn.Open();

          var record = scaffold.LoadRecord(query, conn);
          var form = scaffold.BuildForm(record, conn);

          Assert.Equal("新宿,sinjuku", form["name_with_code"].Value.First());
        }
      }))();
    }

    [Fact]
    public void TestSecretSave()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSecretSave(db);
        ExecSql(db);
      }
    }

    private void RunSecretSave(TestDb db)
    {
      var scaffold = Test.Scaffold.Shop.Create(db.Adapter);

      var query = new NameValueCollection();

      var post = new NameValueCollection();
      post.Set("name", "foobar");
      post.Set("area_id", "1");
      post.Set("password", "1234");
      post.Set("created_at", "");

      string savedId = null;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();
        var record = scaffold.LoadRecord(query, conn);

        //ここで最初のBind
        var form = scaffold.BuildForm(record, conn);

        //2度目のBind
        form.Bind(post);
        form.ExecValidators();

        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, form.ToNameValueCollection(), conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);
        Assert.Equal("HASH@1234", savedRecord.GetString("password"));
      }

      query = new NameValueCollection();
      query.Set("id", savedId);

      post = new NameValueCollection();
      post.Set("name", "test");
      post.Set("area_id", "1");
      post.Set("password", "");
      post.Set("created_at", "2014-02-27 00:00:00");

      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();
        var record = scaffold.LoadRecord(query, conn);

        //ここで最初のBind
        var form = scaffold.BuildForm(record, conn);

        //2度目のBind
        form.Bind(post);
        form.ExecValidators();

        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, form.ToNameValueCollection(), conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);
        Assert.Equal("HASH@1234", savedRecord.GetString("password"));
      }
    }

    [Fact]
    public void TestAutoValidate()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunAutoValidate(db);
        ExecSql(db);
      }
    }

    private void RunAutoValidate(TestDb db)
    {
      var scaffold = Test.Scaffold.Shop.Create(db.Adapter);

      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();
        var record = scaffold.LoadRecord(new NameValueCollection(), conn);

        var form = scaffold.BuildForm(record, conn);


        var nameValidators = form["name"].Validators.ToList();
        Assert.Equal(1, nameValidators.Count);
        
        Assert.IsType<Sdx.Validation.NotEmpty>(nameValidators[0]);
        //Assert.IsType<Sdx.Validation.StringLength>(nameValidators[1]);
        //Assert.Equal(null, ((Sdx.Validation.StringLength)nameValidators[1]).Min);
        //Assert.Equal(100, ((Sdx.Validation.StringLength)nameValidators[1]).Max);
      }
    }

    [Fact]
    public void TestBindRelation()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunBindRelation(db);
        ExecSql(db);
      }
    }

    private void RunBindRelation(TestDb db)
    {
      Func<Sdx.Db.Connection, string, string[], Sdx.Db.Record> resetCategory = (conn, id, categories) =>
      {
        var record = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, id);
        var currentRecords = record.GetRecordSet("shop_category", conn);
        currentRecords.ForEach(crec => crec.Delete(conn));

        foreach (var category_id in categories)
        {
          var shop_category = new Test.Orm.ShopCategory();
          shop_category.SetValue("shop_id", record.GetString("id"));
          shop_category.SetValue("category_id", category_id);
          conn.BeginTransaction();
          try
          {
            shop_category.Save(conn);
            conn.Commit();
          }
          catch (Exception)
          {
            conn.Rollback();
            throw;
          }
        }

        return record;
      };

      Action<Sdx.Db.Connection, Sdx.Html.Form, string[]> assertCheckbox = (conn, form, categories) =>
      {
        var checkboxies = ((Sdx.Html.CheckableGroup)form["category_id"]).Checkables;
        //categoriesにあるものは空ではない
        foreach (var cid in categories)
        {
          Assert.False(checkboxies.First(ck => ck.Tag.Attr["value"] == cid).Value.IsEmpty);
        }

        //categoriesに無いものは空。
        foreach (var checkbox in checkboxies.Where(ck => categories.All(cid => ck.Tag.Attr["value"] != cid)))
        {
          Assert.True(checkbox.Value.IsEmpty);
        }
      };

      var scaffold = Test.Scaffold.Shop.Create(db.Adapter);
      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var categories = new string[] { "1", "2" };
        var record = resetCategory(conn, "1", categories);
        record.ClearRecordCache();

        var form = scaffold.BuildForm(record, conn);
        assertCheckbox(conn, form, categories);
      }

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var categories = new string[] { };
        var record = resetCategory(conn, "1", categories);
        record.ClearRecordCache();

        var form = scaffold.BuildForm(record, conn);
        assertCheckbox(conn, form, categories);
      }

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var categories = new string[] { "3", "4", "5" };
        var record = resetCategory(conn, "1", categories);
        record.ClearRecordCache();

        var form = scaffold.BuildForm(record, conn);
        assertCheckbox(conn, form, categories);
      }
    }

    [Fact]
    public void TestDelete()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunDelete(db);
        ExecSql(db);
      }
    }

    private void RunDelete(TestDb db)
    {
      var scaffold = Test.Scaffold.Shop.Create(db.Adapter);

      var query = new NameValueCollection();

      var post = new NameValueCollection();
      post.Set("name", "foobar");
      post.Set("area_id", "1");
      post.Add("category_id", "2");
      post.Add("category_id", "3");

      string savedId;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        var record = scaffold.LoadRecord(query, conn);
        var form = scaffold.BuildForm(record, conn);

        conn.BeginTransaction();
        try
        {
          scaffold.Save(record, post, conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //確認する
        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);
        Assert.Equal("foobar", savedRecord.GetString("name"));
        Assert.Equal("1", savedRecord.GetString("area_id"));

        var shopCategories = savedRecord.GetRecordSet("shop_category", conn, select => select.AddOrder("category_id", Sdx.Db.Sql.Order.ASC));
        Assert.Equal(2, shopCategories.Count);
        Assert.Equal("2", shopCategories[0].GetString("category_id"));
        Assert.Equal("3", shopCategories[1].GetString("category_id"));
      }

      query = new NameValueCollection();
      query.Set("id", savedId);

      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        var record = scaffold.LoadRecord(query, conn);
        var pkeyJson = Sdx.Util.Json.Encoder(record.GetPkeyValues());

        conn.BeginTransaction();
        try
        {
          scaffold.DeleteRecord(pkeyJson, conn);
          conn.Commit();
        }
        catch (Exception)
        {
          conn.Rollback();
          throw;
        }

        //確認する
        savedId = record.GetString("id");
        var savedRecord = (new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, savedId);
        Assert.Null(savedRecord);
      }
    }

    [Fact]
    public void TestPerPage()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunPerPage(db);
        ExecSql(db);
      }
    }

    private void RunPerPage(TestDb db)
    {
      InitHttpContextMock("pid=1");
      var scaffold = Test.Scaffold.Shop.Create(db.Adapter);
      scaffold.PerPage = 2;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        Assert.True(scaffold.HasPerPage);

        var pagerLink = new Sdx.Html.PagerLink(new Sdx.Pager(scaffold.PerPage), "pid");
        var recordSet = scaffold.FetchRecordSet(conn, pagerLink.Pager);
        Assert.Equal(1, pagerLink.Pager.Page);
        Assert.Equal(scaffold.PerPage, recordSet.Count);
        Assert.Equal(1, recordSet[0].GetValue("id"));
        Assert.Equal(2, recordSet[1].GetValue("id"));
      }

      InitHttpContextMock("pid=2");
      scaffold = Test.Scaffold.Shop.Create(db.Adapter);
      scaffold.PerPage = 2;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        Assert.True(scaffold.HasPerPage);

        var pagerLink = new Sdx.Html.PagerLink(new Sdx.Pager(scaffold.PerPage), "pid");
        var recordSet = scaffold.FetchRecordSet(conn, pagerLink.Pager);
        Assert.Equal(2, pagerLink.Pager.Page);
        Assert.Equal(scaffold.PerPage, recordSet.Count);
        Assert.Equal(3, recordSet[0].GetValue("id"));
        Assert.Equal(4, recordSet[1].GetValue("id"));
      }

      //クエリーなし
      InitHttpContextMock("");
      scaffold = Test.Scaffold.Shop.Create(db.Adapter);
      scaffold.PerPage = 2;
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();

        Assert.True(scaffold.HasPerPage);

        var pagerLink = new Sdx.Html.PagerLink(new Sdx.Pager(scaffold.PerPage), "pid");
        var recordSet = scaffold.FetchRecordSet(conn, pagerLink.Pager);
        Assert.Equal(1, pagerLink.Pager.Page);
        Assert.Equal(scaffold.PerPage, recordSet.Count);
        Assert.Equal(1, recordSet[0].GetValue("id"));
        Assert.Equal(2, recordSet[1].GetValue("id"));
      }
    }

    [Fact]
    public void TestAutoCurrent()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunAutoCurrent(db);
        ExecSql(db);
      }
    }

    private void RunAutoCurrent(TestDb db)
    {
      InitHttpContextMock("");
      var scaffold = Test.Scaffold.Shop.Create(db.Adapter);
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();
        var record = scaffold.LoadRecord(HttpContext.Current.Request.Params, conn);
        var form = scaffold.BuildForm(record, conn);

        Assert.IsType<Sdx.Html.CheckBoxGroup>(form["auto_created_at"]);

        Assert.True(record.IsNew);
        Assert.Equal(
          HtmlLiner(@"
<span>
  <label><input type=""checkbox"" value=""1"" name=""auto_created_at"" checked>現在日時で更新</label>
</span>"),
          form["auto_created_at"].Tag.Render()
        );

        var values = new NameValueCollection();

        scaffold.BindToForm(form, values);
        Assert.Equal("", form["created_at"].Value.ToString());

        values.Add("auto_created_at", "1");
        scaffold.BindToForm(form, values);
        Assert.NotEqual("", form["created_at"].Value.ToString());
      }

      InitHttpContextMock("id=1");
      scaffold = Test.Scaffold.Shop.Create(db.Adapter);
      using (var conn = scaffold.Db.CreateConnection())
      {
        conn.Open();
        var record = scaffold.LoadRecord(HttpContext.Current.Request.Params, conn);
        var form = scaffold.BuildForm(record, conn);

        Assert.False(record.IsNew);
        Assert.Equal(
          HtmlLiner(@"
<span>
  <label><input type=""checkbox"" value=""1"" name=""auto_created_at"">現在日時で更新</label>
</span>"),
          form["auto_created_at"].Tag.Render()
        );

        var values = new NameValueCollection();

        scaffold.BindToForm(form, values);
        Assert.Equal("", form["created_at"].Value.ToString());

        values.Add("auto_created_at", "1");
        scaffold.BindToForm(form, values);
        Assert.NotEqual("", form["created_at"].Value.ToString());
      }
    }

    [Fact]
    public void TestPostSaveHook()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunPostSaveHook(db);
        ExecSql(db);
      }
    }

    private void RunPostSaveHook(TestDb db)
    {
      var epoch = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0);
      var timeStamp = (long)epoch.TotalSeconds;//少数点以下は丸める

      //仮想送信パラメータ
      //テストの度に新しいレコードができるようにするためタイムスタンプを名前に付けてます
      var requestForm = new NameValueCollection()
      {
        {"name", "test_area" + timeStamp.ToString()},
        {"code", "test_code" + timeStamp.ToString()}
      };

      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter);

      //form
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("name"))
          .Set("label", new Sdx.Scaffold.Config.Value("名称"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("code"))
          .Set("label", new Sdx.Scaffold.Config.Value("コード"))
        );

    }
  }
}
