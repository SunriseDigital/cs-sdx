using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

namespace UnitTest
{
  [TestClass]
  public class DbQueryTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestFactorySimpleRetrieve()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFactorySimpleRetrieve(db);
      }
    }

    private void RunFactorySimpleRetrieve(TestDb db)
    {
      var con = db.Adapter.CreateConnection();
      using (con)
      {
        con.Open();


        DbCommand command = con.CreateCommand();
        command.CommandText = "SELECT shop.name as name_shop, area.name as name_area FROM shop"
          + " INNER JOIN area ON area.id = shop.area_id"
          + " WHERE shop.id = @shop@id"
          ;

        command.Parameters.Add(db.Adapter.CreateParameter("@shop@id", "1"));

        DbDataAdapter adapter = db.Adapter.CreateDataAdapter();
        DataSet dataset = new DataSet();

        adapter.SelectCommand = command;
        adapter.Fill(dataset);

        Assert.Equal(1, dataset.Tables[0].Rows.Count);
        Assert.Equal("天祥", dataset.Tables[0].Rows[0]["name_shop"]);
        Assert.Equal("新中野", dataset.Tables[0].Rows[0]["name_area"]);
      }
    }

    [Fact]
    public void TestSelectSimple()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSimple(db);
        ExecSql(db);
      }
    }

    private void RunSelectSimple(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();

      //AddColumn
      select.AddFrom("shop").AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop", "s").AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}s{1}.* FROM {0}shop{1} AS {0}s{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("s").AddFrom("shop");
      select.Context("shop").AddColumns("id");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //SetColumns
      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns("id");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns("id", "name");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns(new String[] { "id", "name" });
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //AddColumns
      select.RemoveContext("shop").AddFrom("shop").AddColumns("id");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").AddColumns("name");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").AddColumns("id", "name");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").AddColumns(new String[] { "id", "name" });
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //ClearColumns
      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns(new String[] { "id", "name" });
      select.Context("shop").ClearColumns().AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").ClearColumns().AddColumns("id");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").ClearColumns().AddColumns("id");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").AddColumns("name");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectSimpleInnerJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSimpleInnerJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectSimpleInnerJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();

      select.AddFrom("shop").AddColumns("*");

      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          )
      ).AddColumns("*");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}area{1}.* FROM {0}shop{1} INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}"),
        db.Command.CommandText
      );

      //上書きなので順番が入れ替わるはず
      select.Context("shop").InnerJoin(
        "image",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("main_image_id", "shop"),
            new Sdx.Db.Query.Column("id", "image")
          )
        );

      //同じテーブルをJOINしてもAliasを与えなければ上書きになる
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          ).Add(
            new Sdx.Db.Query.Column("id", "area"),
            1
          )
      ).AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}area{1}.* FROM {0}shop{1} INNER JOIN {0}image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image{1}.{0}id{1} INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} AND {0}area{1}.{0}id{1} = @0"),
        db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters["@0"].Value);
    }

    [Fact]
    public void TestSelectMultipleInnerJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectMultipleInnerJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectMultipleInnerJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .InnerJoin("area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          )
        ).InnerJoin("large_area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("large_area_id", "area"),
            new Sdx.Db.Query.Column("id", "large_area")
          )
        );

      select.Context("shop").AddColumns("*");

      select.Adapter = db.Adapter;
      db.Command = select.Build();

      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}"),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectNoColumnInnerJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectNoColumnInnerJoin(db);

        //DbExceptionが投げられたかチェックする
        Exception ex = Record.Exception(() => ExecSql(db));
        Assert.True(ex.GetType().IsSubclassOf(typeof(DbException)));
      }
    }

    private void RunSelectNoColumnInnerJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .InnerJoin("area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          )
        )
        .InnerJoin("large_area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("large_area_id", "area"),
            new Sdx.Db.Query.Column("id", "large_area")
          )
        );

      select.Adapter = db.Adapter;
      db.Command = select.Build();

      Assert.Equal(
        db.Sql("SELECT FROM {0}shop{1} INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}"),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectSameTableInnerJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSameTableInnerJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectSameTableInnerJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");

      select.Context("shop")
        .InnerJoin("image", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("main_image_id", "shop"),
            new Sdx.Db.Query.Column("id", "main_image")
          ), "main_image")
        .AddColumns("*");

      select.Context("shop")
        .InnerJoin("image", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("sub_image_id", "shop"),
            new Sdx.Db.Query.Column("id", "sub_image")
          ), "sub_image")
        .AddColumns("*");

      select.Adapter = db.Adapter;
      db.Command = select.Build();

      Assert.Equal(
        db.Sql(
          @"SELECT {0}shop{1}.*, {0}main_image{1}.*, {0}sub_image{1}.* 
            FROM {0}shop{1} 
            INNER JOIN {0}image{1} AS {0}main_image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}main_image{1}.{0}id{1} 
            INNER JOIN {0}image{1} AS {0}sub_image{1} ON {0}shop{1}.{0}sub_image_id{1} = {0}sub_image{1}.{0}id{1}"
        ),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectColumnAlias()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectColumnAlias(db);
        ExecSql(db);
      }
    }

    private void RunSelectColumnAlias(TestDb db)
    {
      var select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("id", "shop_id");

      select.Adapter = db.Adapter;
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} AS {0}shop_id{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      select.Context("shop")
        .ClearColumns()
        .AddColumns(new Dictionary<string, object>()
         {
           {"shop_id", "id"},
           {"shop_name", "name"},
         });

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} AS {0}shop_id{1}, {0}shop{1}.{0}name{1} AS {0}shop_name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      select.Context("shop")
        .ClearColumns()
        .AddColumns(new Dictionary<string, object>()
         {
           {"shop_id", "id"},
           {"shop_name", Sdx.Db.Query.Expr.Wrap("name")},
         });

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} AS {0}shop_id{1}, {0}shop{1}.name AS {0}shop_name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectLeftJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectLeftJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectLeftJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select.AddFrom("shop")
        .AddColumn("*")
        .LeftJoin("image", db.Adapter.CreateCondition().Add(
            new Sdx.Db.Query.Column("main_image_id", "shop"),
            new Sdx.Db.Query.Column("id", "image")
        ));

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} LEFT JOIN {0}image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image{1}.{0}id{1}"),
       db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectJoinOrder()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectJoinOrder(db);
        ExecSql(db);
      }
    }

    private void RunSelectJoinOrder(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      var cshop = new Sdx.Db.Query.Column("main_image_id", "shop");
      var cols = new Dictionary<int, Sdx.Db.Query.Column>();
      for (int i = 1; i <= 7; i++)
      {
        cols[i] = new Sdx.Db.Query.Column("id", "image" + i.ToString());
      }

      select.AddFrom("shop").AddColumn("*");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[1]), "image1");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[2]), "image2");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[3]), "image3");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[4]), "image4");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[5]), "image5");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[6]), "image6");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[7]), "image7");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.* 
          FROM {0}shop{1} 
          INNER JOIN {0}image{1} AS {0}image3{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image3{1}.{0}id{1} 
          INNER JOIN {0}image{1} AS {0}image5{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image5{1}.{0}id{1} 
          INNER JOIN {0}image{1} AS {0}image7{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image7{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image1{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image1{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image2{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image2{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image4{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image4{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image6{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image6{1}.{0}id{1}"
        ),
        db.Command.CommandText
      );

      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("*");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[1]), "image1");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[2]), "image2");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[3]), "image3");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[4]), "image4");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[5]), "image5");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[6]), "image6");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[7]), "image7");

      select.JoinOrder = Sdx.Db.Query.JoinOrder.Natural;

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
        db.Sql(@"SELECT {0}shop{1}.* 
          FROM {0}shop{1} 
          LEFT JOIN {0}image{1} AS {0}image1{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image1{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image2{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image2{1}.{0}id{1} 
          INNER JOIN {0}image{1} AS {0}image3{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image3{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image4{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image4{1}.{0}id{1} 
          INNER JOIN {0}image{1} AS {0}image5{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image5{1}.{0}id{1} 
          LEFT JOIN {0}image{1} AS {0}image6{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image6{1}.{0}id{1} 
          INNER JOIN {0}image{1} AS {0}image7{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image7{1}.{0}id{1}"),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectNonTableColumns()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectNonTableColumns(db);
        ExecSql(db);
      }
    }

    private void RunSelectNonTableColumns(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();

      //単純なAddColumns
      select.AddFrom("shop");
      select.AddColumns("id", "name");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1}, {0}name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //テーブル名だけすり替える
      select.RemoveContext("shop").AddFrom("area");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1}, {0}name{1} FROM {0}area{1}"),
       db.Command.CommandText
      );

      //SetColumns
      select.RemoveContext("area").AddFrom("shop");
      select.ClearColumns().AddColumns("name", "area_id");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}name{1}, {0}area_id{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //AddColumn MAX
      select.ClearColumns().RemoveContext("shop").AddFrom("shop");
      select.AddColumn(
        Sdx.Db.Query.Expr.Wrap("MAX(" + select.Context("shop").AppendName("id") + ")"),
        "max_id"
      );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT MAX({0}shop{1}.{0}id{1}) AS {0}max_id{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectMultipleFrom()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectMultipleFrom(db);
        ExecSql(db);
      }
    }

    private void RunSelectMultipleFrom(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("*");
      select.AddFrom("area");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}, {0}area{1}"),
       db.Command.CommandText
      );
    }

    [Fact]
    public void TrySqlAction()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        var command = db.Adapter.CreateCommand();
        command.CommandText = db.Sql("SELECT * FROM shop WHERE {0}area_id{1} IN (SELECT id FROM area WHERE id = 1)");
        this.ExecCommand(command, db.Adapter);
      }
    }

    [Fact]
    public void TestSelectWhereSimple()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectWhereSimple(db);
        ExecSql(db);
      }
    }

    private void RunSelectWhereSimple(TestDb db)
    {
      Sdx.Db.Query.Select select;

      //selectに対する呼び出し
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("*");

      select.Where.Add("id", "1");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);


      //tableに対する呼び出し
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("*");

      select.Context("shop").Where.Add("id", "1");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);

      //WhereのAdd
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("*");

      select.Where.Add(
        db.Adapter.CreateCondition()
          .Add("id", "1")
          .AddOr("id", "2")
      );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE ({0}id{1} = @0 OR {0}id{1} = @1)"),
       db.Command.CommandText
      );

      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);
      Assert.Equal("2", db.Command.Parameters[1].Value);

      //Where2つをORでつなぐ
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumn("*");

      select.Where
        .Add(
          db.Adapter.CreateCondition()
            .Add("id", "3")
            .Add("id", "4")
        ).AddOr(
          db.Adapter.CreateCondition()
            .Add("id", "1")
            .AddOr("id", "2")
        );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE ({0}id{1} = @0 AND {0}id{1} = @1) OR ({0}id{1} = @2 OR {0}id{1} = @3)"),
       db.Command.CommandText
      );

      Assert.Equal(4, db.Command.Parameters.Count);
      Assert.Equal("3", db.Command.Parameters[0].Value);
      Assert.Equal("4", db.Command.Parameters[1].Value);
      Assert.Equal("1", db.Command.Parameters[2].Value);
      Assert.Equal("2", db.Command.Parameters[3].Value);
    }

    [Fact]
    public void TestSelectRawSubqueryJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectRawSubqueryJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectRawSubqueryJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .InnerJoin(
          Sdx.Db.Query.Expr.Wrap("(SELECT id FROM area WHERE id = 1)"),
          db.Adapter.CreateCondition().Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "sub_cat")
          ),
          "sub_cat"
        );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN (SELECT id FROM area WHERE id = 1) AS {0}sub_cat{1} ON {0}shop{1}.{0}area_id{1} = {0}sub_cat{1}.{0}id{1}"),
       db.Command.CommandText
      );

      Assert.Equal(0, db.Command.Parameters.Count);
    }

    [Fact]
    public void TestSelectSubqueryJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSubqueryJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectSubqueryJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = new Sdx.Db.Query.Select();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").InnerJoin(
        sub,
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Query.Column("area_id", "shop"),
          new Sdx.Db.Query.Column("id", "sub_cat")
        ), "sub_cat");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}id{1} = @0) AS {0}sub_cat{1} ON {0}shop{1}.{0}area_id{1} = {0}sub_cat{1}.{0}id{1} WHERE {0}shop{1}.{0}id{1} = @1"),
       db.Command.CommandText
      );

      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("2", db.Command.Parameters[0].Value);//サブクエリのWhereの方が先にAddされる
      Assert.Equal("1", db.Command.Parameters[1].Value);
    }

    [Fact]
    public void TestSelectSubqueryLeftJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSubqueryLeftJoin(db);
        ExecSql(db);
      }
    }

    private void RunSelectSubqueryLeftJoin(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = new Sdx.Db.Query.Select();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").LeftJoin(sub, db.Adapter.CreateCondition().Add(
        new Sdx.Db.Query.Column("area_id", "shop"),
        new Sdx.Db.Query.Column("id", "sub_cat")
      ), "sub_cat");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} LEFT JOIN (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}id{1} = @0) AS {0}sub_cat{1} ON {0}shop{1}.{0}area_id{1} = {0}sub_cat{1}.{0}id{1} WHERE {0}shop{1}.{0}id{1} = @1"),
       db.Command.CommandText
      );

      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("2", db.Command.Parameters[0].Value);//サブクエリのWhereの方が先にAddされる
      Assert.Equal("1", db.Command.Parameters[1].Value);
    }

    [Fact]
    public void TestSelectSubqueryWhere()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSubqueryWhere(db);
        ExecSql(db);
      }
    }

    private void RunSelectSubqueryWhere(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = new Sdx.Db.Query.Select();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").Where.Add("area_id", sub, Sdx.Db.Query.Comparison.In);

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} = @0 AND {0}shop{1}.{0}area_id{1} IN (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}id{1} = @1)"),
       db.Command.CommandText
      );

      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);
      Assert.Equal("2", db.Command.Parameters[1].Value);
    }

    [Fact]
    public void TestSelectRawSubqueryFrom()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectRawSubqueryFrom(db);
        ExecSql(db);
      }
    }

    private void RunSelectRawSubqueryFrom(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      select.AddFrom(
        Sdx.Db.Query.Expr.Wrap("(SELECT id FROM area WHERE id = 1)"),
        "sub_cat"
      );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}, (SELECT id FROM area WHERE id = 1) AS {0}sub_cat{1} WHERE {0}shop{1}.{0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);
    }

    [Fact]
    public void TestSelectSubqueryFrom()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSubqueryFrom(db);
        ExecSql(db);
      }
    }

    private void RunSelectSubqueryFrom(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = new Sdx.Db.Query.Select();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.AddFrom(sub, "sub_cat");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}, (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}id{1} = @0) AS {0}sub_cat{1} WHERE {0}shop{1}.{0}id{1} = @1"),
       db.Command.CommandText
      );

      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("2", db.Command.Parameters[0].Value);//サブクエリーのWhereの方が先にAddされる
      Assert.Equal("1", db.Command.Parameters[1].Value);
    }

    [Fact]
    public void TestSelectWhereIn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectWhereIn(db);
        ExecSql(db);
      }
    }

    private void RunSelectWhereIn(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where
          .Add("id", new string[] { "1", "2" })
          .AddOr("id", new string[] { "3", "4" });

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} IN (@0, @1) OR {0}shop{1}.{0}id{1} IN (@2, @3)"),
       db.Command.CommandText
      );

      Assert.Equal(4, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);
      Assert.Equal("2", db.Command.Parameters[1].Value);
      Assert.Equal("3", db.Command.Parameters[2].Value);
      Assert.Equal("4", db.Command.Parameters[3].Value);
    }

    [Fact]
    public void TestSelectGroupHaving()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectGroupHaving(db);
        ExecSql(db);
      }
    }

    private void RunSelectGroupHaving(TestDb db)
    {
      //TableにGroup
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .AddGroup("id");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1}"),
       db.Command.CommandText
      );

      select.Having.Add(
        Sdx.Db.Query.Expr.Wrap("SUM(shop.id)"),
        10,
        Sdx.Db.Query.Comparison.GreaterEqual
      );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1} HAVING SUM(shop.id) >= @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("10", db.Command.Parameters[0].Value);

      //selectに直接
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop");

      select
        .AddColumn("id")
        .AddGroup("id");

      select.Having.Add(
        Sdx.Db.Query.Expr.Wrap("SUM(id)"),
        20,
        Sdx.Db.Query.Comparison.GreaterEqual
      );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1} FROM {0}shop{1} GROUP BY {0}id{1} HAVING SUM(id) >= @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("20", db.Command.Parameters[0].Value);
    }

    [Fact]
    public void TestOrderSelectLimitOffset()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectOrderLimitOffset(db);
        ExecSql(db);
      }
    }

    private void RunSelectOrderLimitOffset(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*");

      select.AddOrder("id", Sdx.Db.Query.Order.DESC);

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC"),
       db.Command.CommandText
      );

      select.SetLimit(100);
      select.Adapter = db.Adapter;
      db.Command = select.Build();

      this.AssertCommandText(
        typeof(Sdx.Db.SqlServerAdapter),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY"),
        db
      );

      this.AssertCommandText(
        typeof(Sdx.Db.MySqlAdapter),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC LIMIT 100"),
        db
      );

      select.SetLimit(100, 10);
      select.Adapter = db.Adapter;
      db.Command = select.Build();

      this.AssertCommandText(
        typeof(Sdx.Db.SqlServerAdapter),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC OFFSET 10 ROWS FETCH NEXT 100 ROWS ONLY"),
        db
      );

      this.AssertCommandText(
        typeof(Sdx.Db.MySqlAdapter),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC LIMIT 100 OFFSET 10"),
        db
      );

    }

    private void AssertCommandText(Type type, string expected, TestDb db)
    {
      Console.WriteLine(type);
      if (type != db.Adapter.GetType())
      {
        return;
      }

      Assert.Equal(expected, db.Command.CommandText);
    }

    [Fact]
    public void TestSelectOrderTable()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectOrderTable(db);
        ExecSql(db);
      }
    }

    private void RunSelectOrderTable(TestDb db)
    {
      var select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .AddOrder("id", Sdx.Db.Query.Order.DESC);

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}shop{1}.{0}id{1} DESC"),
       db.Command.CommandText
      );

      Assert.Equal(0, db.Command.Parameters.Count);
    }

    [Fact]
    public void TestSelectHavingTable()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectHavingTable(db);
        ExecSql(db);
      }
    }

    private void RunSelectHavingTable(TestDb db)
    {
      var select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .AddGroup("id")
        .Having.Add("id", "2", Sdx.Db.Query.Comparison.GreaterEqual);

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1} HAVING {0}shop{1}.{0}id{1} >= @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("2", db.Command.Parameters["@0"].Value);
    }

    [Fact]
    public void TestJoinCondition()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunJoinCondition(db);
        ExecSql(db);
      }
    }

    private void RunJoinCondition(TestDb db)
    {
      Sdx.Db.Query.Select select = new Sdx.Db.Query.Select();

      //InnerJoin
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Query.Column("area_id", "shop"),
          new Sdx.Db.Query.Column("id", "area")
        )
      ).AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} 
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}"), db.Command.CommandText);

      Assert.Equal(0, db.Command.Parameters.Count);

      //InnerJoin Additional String condition
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          ).Add(
            new Sdx.Db.Query.Column("id", "area"),
            "1"
          )
      ).AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} 
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
          AND {0}area{1}.{0}id{1} = @0"), db.Command.CommandText);

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);

      ////InnerJoin Additional Expr condition
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          ).Add(
            new Sdx.Db.Query.Column("id", "area"),
            Sdx.Db.Query.Expr.Wrap(99)
          )
      ).AddColumns("*");
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} 
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
          AND {0}area{1}.{0}id{1} = 99"), db.Command.CommandText);

      Assert.Equal(0, db.Command.Parameters.Count);

      //InnerJoin Additional Subquery
      var sub = new Sdx.Db.Query.Select();
      sub.AddFrom("area").AddColumn("id");
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          ).Add(
            new Sdx.Db.Query.Column("id", "area"),
            sub,
            Sdx.Db.Query.Comparison.In
          )
      ).AddColumns("*");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.*
        FROM {0}shop{1} 
        INNER JOIN {0}area{1}
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
            AND {0}area{1}.{0}id{1} IN (SELECT {0}area{1}.{0}id{1} FROM {0}area{1})"), db.Command.CommandText);

      ////InnerJoin Additional Subquery and parameter
      sub = new Sdx.Db.Query.Select();
      sub.AddFrom("area").AddColumn("id").Where.Add("code", "foo");
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*").Where.Add("name", "bar");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Query.Column("area_id", "shop"),
            new Sdx.Db.Query.Column("id", "area")
          ).Add(
            new Sdx.Db.Query.Column("id", "area"),
            sub,
            Sdx.Db.Query.Comparison.In
          )
      ).AddColumns("*").Where.Add("id", "99");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.*
        FROM {0}shop{1}
        INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
          AND {0}area{1}.{0}id{1} IN 
            (SELECT {0}area{1}.{0}id{1} FROM {0}area{1} WHERE {0}area{1}.{0}code{1} = @0)
        WHERE {0}shop{1}.{0}name{1} = @1 
        AND {0}area{1}.{0}id{1} = @2"), db.Command.CommandText);

      Assert.Equal(3, db.Command.Parameters.Count);
      Assert.Equal("foo", db.Command.Parameters[0].Value);
      Assert.Equal("bar", db.Command.Parameters[1].Value);
      Assert.Equal("99", db.Command.Parameters[2].Value);

      ////InnerJoin Addtional include `OR` right
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop")
        .InnerJoin(
          "area",
          db.Adapter.CreateCondition()
            .Add(
              new Sdx.Db.Query.Column("area_id", "shop"),
              new Sdx.Db.Query.Column("id", "area")
            ).Add(
              db.Adapter.CreateCondition()
                .Add(new Sdx.Db.Query.Column("id", "area"), "1")
                .AddOr(new Sdx.Db.Query.Column("id", "area"), "2")
            )
        )
        .AddColumns("*");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1}
        INNER JOIN {0}area{1}
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
            AND ({0}area{1}.{0}id{1} = @0 OR {0}area{1}.{0}id{1} = @1)"), db.Command.CommandText);

      //InnerJoin Addtional include `OR` left
      select = new Sdx.Db.Query.Select();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop")
        .InnerJoin(
          "area",
          db.Adapter.CreateCondition()
            .Add(
              new Sdx.Db.Query.Column("area_id", "shop"),
              new Sdx.Db.Query.Column("id", "area")
            ).Add(
              db.Adapter.CreateCondition()
                .Add(new Sdx.Db.Query.Column("id", "shop"), "1")
                .AddOr(new Sdx.Db.Query.Column("id", "shop"), "2")
            )
        )
        .AddColumns("*");

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1}
        INNER JOIN {0}area{1}
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
            AND ({0}shop{1}.{0}id{1} = @0 OR {0}shop{1}.{0}id{1} = @1)"), db.Command.CommandText);
    }

    [Fact]
    public void TestSelectNestingWhere()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectNestingWhere(db);
        ExecSql(db);
      }
    }

    private void RunSelectNestingWhere(TestDb db)
    {
      var select = new Sdx.Db.Query.Select();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .InnerJoin("area", db.Adapter.CreateCondition().Add(
          new Sdx.Db.Query.Column("area_id", "shop"),
          new Sdx.Db.Query.Column("id", "area")
         ))
        .InnerJoin("large_area", db.Adapter.CreateCondition().Add(
          new Sdx.Db.Query.Column("large_area_id", "area"),
          new Sdx.Db.Query.Column("id", "large_area")
        ));

      select.Context("shop").Where.Add(
        db.Adapter.CreateCondition().Add(
          db.Adapter.CreateCondition()
            .Add("id", "1")
            .AddOr("id", "2")
        ).Add(
          db.Adapter.CreateCondition()
            .Add("id", "3")
            .AddOr("id", "4")
        )
      );

      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql(@"SELECT
                {0}shop{1}.*
              FROM {0}shop{1}
              INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
              INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
              WHERE (({0}shop{1}.{0}id{1} = @0 OR {0}shop{1}.{0}id{1} = @1) AND ({0}shop{1}.{0}id{1} = @2 OR {0}shop{1}.{0}id{1} = @3))"),
       db.Command.CommandText
      );

      Assert.Equal(4, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters["@0"].Value);
      Assert.Equal("2", db.Command.Parameters["@1"].Value);
      Assert.Equal("3", db.Command.Parameters["@2"].Value);
      Assert.Equal("4", db.Command.Parameters["@3"].Value);

      select.Context("area").Where.Add(
        db.Adapter.CreateCondition().Add(
          db.Adapter.CreateCondition()
            .Add("id", "5")
            .AddOr("id", "6")
        ).Add(
          db.Adapter.CreateCondition()
            .Add("id", "7")
            .AddOr("id", "8")
        )
      );
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql(@"SELECT
                  {0}shop{1}.*
                FROM {0}shop{1}
                INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
                INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
                WHERE
                  (({0}shop{1}.{0}id{1} = @0 OR {0}shop{1}.{0}id{1} = @1)
                  AND
                  ({0}shop{1}.{0}id{1} = @2 OR {0}shop{1}.{0}id{1} = @3))
                AND
                  (({0}area{1}.{0}id{1} = @4 OR {0}area{1}.{0}id{1} = @5)
                  AND
                  ({0}area{1}.{0}id{1} = @6 OR {0}area{1}.{0}id{1} = @7))"),
       db.Command.CommandText
      );
      Assert.Equal(8, db.Command.Parameters.Count);
      Assert.Equal("5", db.Command.Parameters["@4"].Value);
      Assert.Equal("6", db.Command.Parameters["@5"].Value);
      Assert.Equal("7", db.Command.Parameters["@6"].Value);
      Assert.Equal("8", db.Command.Parameters["@7"].Value);

      select.Context("large_area").Where.Add(
        db.Adapter.CreateCondition()
          .Add("id", "9")
          .AddOr("id", "10")
      );
      select.Adapter = db.Adapter;
      db.Command = select.Build();
      Assert.Equal(
       db.Sql(@"SELECT
                  {0}shop{1}.*
                FROM {0}shop{1}
                INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
                INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
                WHERE
                  (({0}shop{1}.{0}id{1} = @0 OR {0}shop{1}.{0}id{1} = @1)
                  AND
                  ({0}shop{1}.{0}id{1} = @2 OR {0}shop{1}.{0}id{1} = @3))
                AND
                  (({0}area{1}.{0}id{1} = @4 OR {0}area{1}.{0}id{1} = @5)
                  AND
                  ({0}area{1}.{0}id{1} = @6 OR {0}area{1}.{0}id{1} = @7))
                AND ({0}large_area{1}.{0}id{1} = @8 OR {0}large_area{1}.{0}id{1} = @9)"),
       db.Command.CommandText
      );
      Assert.Equal(10, db.Command.Parameters.Count);
      Assert.Equal("9", db.Command.Parameters["@8"].Value);
      Assert.Equal("10", db.Command.Parameters["@9"].Value);
    }
  }
}
