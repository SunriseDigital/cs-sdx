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

namespace UnitTest
{
  using Config = Sdx.Scaffold.Config;
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter, db.Adapter.ToString());
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("column", new Sdx.Scaffold.Config.Value("id"))
          .Set("label", new Sdx.Scaffold.Config.Value("ID"))
        ).Add(Sdx.Scaffold.Config.Item.Create()
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

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Config.Value("name"));

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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Config.Value("name"));

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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Config.Value("name"), new Config.Value("FetchPairsForOption"));

        scaffold.Group.Init();

        Assert.True(scaffold.Group.HasSelector);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();


          var select = scaffold.Group.BuildSelector(conn);
          Assert.IsType<Sdx.Html.Select>(select);
          Assert.Equal("<select name=\"large_area_id\"><option value=\"\">全て</option><option value=\"1\" selected>東京</option><option value=\"2\">愛知</option></select>", select.Tag.Render());

          List<Sdx.Html.Option> options = (List<Sdx.Html.Option>)select.Options;
          Assert.Equal("", options[0].Value.ToString());//最初は空

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

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Config.Value("name"));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Config.Value("name"));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.Table.LargeArea.Meta, new Config.Value("name"), new Config.Value("FetchPairsForOption"));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"));

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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"));

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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"), new Config.Value("GetList"));

        scaffold.Group.Init();

        Assert.True(scaffold.Group.HasSelector);

        using (var conn = db.Adapter.CreateConnection())
        {
          conn.Open();

          var select = scaffold.Group.BuildSelector(conn);
          Assert.IsType<Sdx.Html.Select>(select);
          Assert.Equal("<select name=\"large_area_id\"><option value=\"\">全て</option><option value=\"1\" selected>東京</option><option value=\"2\">愛知</option></select>", select.Tag.Render());

          List<Sdx.Html.Option> options = (List<Sdx.Html.Option>)select.Options;
          Assert.Equal("", options[0].Value.ToString());//最初は空

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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"), new Config.Value("GetList"));
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
        var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
        scaffold.EditPageUrl = new Sdx.Web.Url("/scaffold/area/edit.aspx");
        scaffold.ListPageUrl = new Sdx.Web.Url("/scaffold/area/list.aspx");

        scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", typeof(Test.Data.LargeArea), new Config.Value("GetName"));
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
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
        Assert.Equal("東京", scaffold.DisplayList[0].Build(records[0], conn));
        Assert.Equal("tokyo", scaffold.DisplayList[1].Build(records[0], conn));
        Assert.Equal("愛知", scaffold.DisplayList[0].Build(records.First(r => r.Get("large_area_id") == 2), conn));
        Assert.Equal("aichi", scaffold.DisplayList[1].Build(records.First(r => r.Get("large_area_id") == 2), conn));
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.LargeArea.Meta, db.Adapter, db.Adapter.ToString());
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
          scaffold.DisplayList[0].Build(records[0], conn)
        );
        Assert.Equal(
          "<a href=\"/path/to/area/list?large_area_id=2\">愛知</a>",
          scaffold.DisplayList[0].Build(records[1], conn)
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Shop.Meta, db.Adapter, db.Adapter.ToString());
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
        Assert.IsType<Sdx.Html.CheckableGroup>(form["category_id"]);

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
        var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Shop(), savedId);
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
        var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Shop(), savedId);

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
        var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Shop(), savedId);

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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
      scaffold.FormList
        .Add(Sdx.Scaffold.Config.Item.Create()
          .Set("label", new Sdx.Scaffold.Config.Value("名前とコード"))
          .Set("column", new Sdx.Scaffold.Config.Value("name_with_code"))
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
        var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Area(), savedId);
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
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.Table.Area.Meta, db.Adapter, db.Adapter.ToString());
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
          var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Area(), savedId);
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
      var scaffold = Test.Scaffold.Shop.Create(db.Adapter, db.Adapter.ToString());

      var query = new NameValueCollection();

      var post = new NameValueCollection();
      post.Set("name", "foobar");
      post.Set("area_id", "1");
      post.Set("password", "1234");

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
        var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Shop(), savedId);
        Assert.Equal("HASH@1234", savedRecord.GetString("password"));
      }

      query = new NameValueCollection();
      query.Set("id", savedId);

      post = new NameValueCollection();
      post.Set("password", "");

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
        var savedRecord = conn.FetchRecordByPkey(new Test.Orm.Table.Shop(), savedId);
        Assert.Equal("HASH@1234", savedRecord.GetString("password"));
      }
    }
  }
}
