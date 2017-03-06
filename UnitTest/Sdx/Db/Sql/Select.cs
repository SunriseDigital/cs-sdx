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
  public class Db_Sql_Select : BaseDbTest
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();

      //AddColumn
      select.AddFrom("shop").AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop", "s").AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}s{1}.* FROM {0}shop{1} AS {0}s{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("s").AddFrom("shop");
      select.Context("shop").AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //SetColumns
      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns("id", "name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns(new String[] { "id", "name" });
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //AddColumns
      select.RemoveContext("shop").AddFrom("shop").AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").AddColumns("name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").AddColumns("id", "name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.RemoveContext("shop").AddFrom("shop").AddColumns(new String[] { "id", "name" });
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //ClearColumns
      select.RemoveContext("shop").AddFrom("shop").ClearColumns().AddColumns(new String[] { "id", "name" });
      select.Context("shop").ClearColumns().AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").ClearColumns().AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").ClearColumns().AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Context("shop").AddColumns("name");
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();

      select.AddFrom("shop").AddColumns("*");

      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          )
      ).AddColumns("*");

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
            new Sdx.Db.Sql.Column("main_image_id", "shop"),
            new Sdx.Db.Sql.Column("id", "image")
          )
        );

      //同じテーブルをJOINしてもAliasを与えなければ上書きになる
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          ).Add(
            new Sdx.Db.Sql.Column("id", "area"),
            1
          )
      ).AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}area{1}.* FROM {0}shop{1} INNER JOIN {0}image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}image{1}.{0}id{1} INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} AND {0}area{1}.{0}id{1} = @0"),
        db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal(1, db.Command.Parameters["@0"].Value);
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .InnerJoin("area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          )
        ).InnerJoin("large_area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("large_area_id", "area"),
            new Sdx.Db.Sql.Column("id", "large_area")
          )
        );

      select.Context("shop").AddColumns("*");

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .InnerJoin("area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          )
        )
        .InnerJoin("large_area", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("large_area_id", "area"),
            new Sdx.Db.Sql.Column("id", "large_area")
          )
        );

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");

      select.Context("shop")
        .InnerJoin("image", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("main_image_id", "shop"),
            new Sdx.Db.Sql.Column("id", "main_image")
          ), "main_image")
        .AddColumns("*");

      select.Context("shop")
        .InnerJoin("image", db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("sub_image_id", "shop"),
            new Sdx.Db.Sql.Column("id", "sub_image")
          ), "sub_image")
        .AddColumns("*");

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
      var select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumn("id", "shop_id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} AS {0}shop_id{1} FROM {0}shop{1}"),
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select.AddFrom("shop")
        .AddColumn("*")
        .LeftJoin("image", db.Adapter.CreateCondition().Add(
            new Sdx.Db.Sql.Column("main_image_id", "shop"),
            new Sdx.Db.Sql.Column("id", "image")
        ));

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      var cshop = new Sdx.Db.Sql.Column("main_image_id", "shop");
      var cols = new Dictionary<int, Sdx.Db.Sql.Column>();
      for (int i = 1; i <= 7; i++)
      {
        cols[i] = new Sdx.Db.Sql.Column("id", "image" + i.ToString());
      }

      select.AddFrom("shop").AddColumn("*");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[1]), "image1");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[2]), "image2");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[3]), "image3");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[4]), "image4");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[5]), "image5");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[6]), "image6");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[7]), "image7");

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

      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumn("*");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[1]), "image1");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[2]), "image2");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[3]), "image3");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[4]), "image4");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[5]), "image5");
      select.Context("shop").LeftJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[6]), "image6");
      select.Context("shop").InnerJoin("image", db.Adapter.CreateCondition().Add(cshop, cols[7]), "image7");

      select.JoinOrder = Sdx.Db.Sql.JoinOrder.Natural;

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();

      //単純なAddColumns
      select.AddFrom("shop");
      select.AddColumns("id", "name");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1}, {0}name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //テーブル名だけすり替える
      select.RemoveContext("shop").AddFrom("area");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1}, {0}name{1} FROM {0}area{1}"),
       db.Command.CommandText
      );

      //SetColumns
      select.RemoveContext("area").AddFrom("shop");
      select.ClearColumns().AddColumns("name", "area_id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}name{1}, {0}area_id{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //AddColumn MAX
      select.ClearColumns().RemoveContext("shop").AddFrom("shop");
      select.AddColumn(
        Sdx.Db.Sql.Expr.Wrap("MAX(" + select.Context("shop").AppendName("id") + ")"),
        "max_id"
      );

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumn("*");
      select.AddFrom("area");

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
      Sdx.Db.Sql.Select select;

      //selectに対する呼び出し
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumn("*");

      select.Where.Add("id", "1");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);


      //tableに対する呼び出し
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumn("*");

      select.Context("shop").Where.Add("id", "1");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);

      //WhereのAdd
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumn("*");

      select.Where.Add(
        db.Adapter.CreateCondition()
          .Add("id", "1")
          .AddOr("id", "2")
      );

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE ({0}id{1} = @0 OR {0}id{1} = @1)"),
       db.Command.CommandText
      );

      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);
      Assert.Equal("2", db.Command.Parameters[1].Value);

      //Where2つをORでつなぐ
      select = db.Adapter.CreateSelect();
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .InnerJoin(
          Sdx.Db.Sql.Expr.Wrap("(SELECT id FROM area WHERE id = 1)"),
          db.Adapter.CreateCondition().Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "sub_cat")
          ),
          "sub_cat"
        ).Where.Add("id", 13);

      db.Command = select.Build();
      Assert.Equal(
       db.Sql(@"SELECT {0}shop{1}.*
                FROM {0}shop{1}
                INNER JOIN 
                  (SELECT id FROM area WHERE id = 1) AS {0}sub_cat{1}
                    ON {0}shop{1}.{0}area_id{1} = {0}sub_cat{1}.{0}id{1}
                WHERE {0}sub_cat{1}.{0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal(13, db.Command.Parameters[0].Value);
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Sql.Select sub = db.Adapter.CreateSelect();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").InnerJoin(
        sub,
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("area_id", "shop"),
          new Sdx.Db.Sql.Column("id", "sub_cat")
        ), "sub_cat").Where.Add("id", 12);

      db.Command = select.Build();
      Assert.Equal(
       db.Sql(@"SELECT {0}shop{1}.*
                  FROM {0}shop{1} 
                  INNER JOIN 
                    (SELECT {0}area{1}.{0}id{1} 
                      FROM {0}area{1} 
                      WHERE {0}area{1}.{0}id{1} = @0) AS {0}sub_cat{1} ON {0}shop{1}.{0}area_id{1} = {0}sub_cat{1}.{0}id{1} 
                  WHERE {0}shop{1}.{0}id{1} = @1 AND {0}sub_cat{1}.{0}id{1} = @2"),
       db.Command.CommandText
      );

      Assert.Equal(3, db.Command.Parameters.Count);
      Assert.Equal("2", db.Command.Parameters[0].Value);//サブクエリのWhereの方が先にAddされる
      Assert.Equal("1", db.Command.Parameters[1].Value);
      Assert.Equal(12, db.Command.Parameters[2].Value);


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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Sql.Select sub = db.Adapter.CreateSelect();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").LeftJoin(sub, db.Adapter.CreateCondition().Add(
        new Sdx.Db.Sql.Column("area_id", "shop"),
        new Sdx.Db.Sql.Column("id", "sub_cat")
      ), "sub_cat");

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Sql.Select sub = db.Adapter.CreateSelect();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Context("shop").Where.Add("area_id", sub, Sdx.Db.Sql.Comparison.In);

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      select.AddFrom(
        Sdx.Db.Sql.Expr.Wrap("(SELECT id FROM area WHERE id = 1)"),
        "sub_cat"
      );

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Sql.Select sub = db.Adapter.CreateSelect();
      sub
        .AddFrom("area")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.AddFrom(sub, "sub_cat");

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .Where
          .Add("id", new string[] { "1", "2" })
          .AddOr("id", new string[] { "3", "4" });

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .AddGroup("id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1}"),
       db.Command.CommandText
      );

      select.Having.Add(
        Sdx.Db.Sql.Expr.Wrap("SUM(shop.id)"),
        10,
        Sdx.Db.Sql.Comparison.GreaterEqual
      );

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1} HAVING SUM(shop.id) >= @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal(10, db.Command.Parameters[0].Value);

      //selectに直接
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop");

      select
        .AddColumn("id")
        .AddGroup("id");

      select.Having.Add(
        Sdx.Db.Sql.Expr.Wrap("SUM(id)"),
        20,
        Sdx.Db.Sql.Comparison.GreaterEqual
      );

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1} FROM {0}shop{1} GROUP BY {0}id{1} HAVING SUM(id) >= @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal(20, db.Command.Parameters[0].Value);
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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*");

      select.AddOrder("id", Sdx.Db.Sql.Order.DESC);

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC"),
       db.Command.CommandText
      );

      select.SetLimit(100);
      db.Command = select.Build();

      this.AssertCommandText(
        typeof(Sdx.Db.Adapter.SqlServer),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY"),
        db
      );

      this.AssertCommandText(
        typeof(Sdx.Db.Adapter.MySql),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC LIMIT 100"),
        db
      );

      select.SetLimit(100, 10);
      db.Command = select.Build();

      this.AssertCommandText(
        typeof(Sdx.Db.Adapter.SqlServer),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC OFFSET 10 ROWS FETCH NEXT 100 ROWS ONLY"),
        db
      );

      this.AssertCommandText(
        typeof(Sdx.Db.Adapter.MySql),
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
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .AddOrder("id", Sdx.Db.Sql.Order.DESC);

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
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .AddGroup("id")
        .Having.Add("id", "2", Sdx.Db.Sql.Comparison.GreaterEqual);

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
      Sdx.Db.Sql.Select select = db.Adapter.CreateSelect();

      //InnerJoin
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("area_id", "shop"),
          new Sdx.Db.Sql.Column("id", "area")
        )
      ).AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} 
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}"), db.Command.CommandText);

      Assert.Equal(0, db.Command.Parameters.Count);

      //InnerJoin Additional String condition
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          ).Add(
            new Sdx.Db.Sql.Column("id", "area"),
            "1"
          )
      ).AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} 
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
          AND {0}area{1}.{0}id{1} = @0"), db.Command.CommandText);

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);

      ////InnerJoin Additional Expr condition
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          ).Add(
            new Sdx.Db.Sql.Column("id", "area"),
            Sdx.Db.Sql.Expr.Wrap(99)
          )
      ).AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} 
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
          AND {0}area{1}.{0}id{1} = 99"), db.Command.CommandText);

      Assert.Equal(0, db.Command.Parameters.Count);

      //InnerJoin Additional Subquery
      var sub = db.Adapter.CreateSelect();
      sub.AddFrom("area").AddColumn("id");
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          ).Add(
            new Sdx.Db.Sql.Column("id", "area"),
            sub,
            Sdx.Db.Sql.Comparison.In
          )
      ).AddColumns("*");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.*
        FROM {0}shop{1} 
        INNER JOIN {0}area{1}
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
            AND {0}area{1}.{0}id{1} IN (SELECT {0}area{1}.{0}id{1} FROM {0}area{1})"), db.Command.CommandText);

      ////InnerJoin Additional Subquery and parameter
      sub = db.Adapter.CreateSelect();
      sub.AddFrom("area").AddColumn("id").Where.Add("code", "foo");
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*").Where.Add("name", "bar");
      select.Context("shop").InnerJoin(
        "area",
        db.Adapter.CreateCondition()
          .Add(
            new Sdx.Db.Sql.Column("area_id", "shop"),
            new Sdx.Db.Sql.Column("id", "area")
          ).Add(
            new Sdx.Db.Sql.Column("id", "area"),
            sub,
            Sdx.Db.Sql.Comparison.In
          )
      ).AddColumns("*").Where.Add("id", "99");

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
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop")
        .InnerJoin(
          "area",
          db.Adapter.CreateCondition()
            .Add(
              new Sdx.Db.Sql.Column("area_id", "shop"),
              new Sdx.Db.Sql.Column("id", "area")
            ).Add(
              db.Adapter.CreateCondition()
                .Add(new Sdx.Db.Sql.Column("id", "area"), "1")
                .AddOr(new Sdx.Db.Sql.Column("id", "area"), "2")
            )
        )
        .AddColumns("*");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT {0}shop{1}.*, {0}area{1}.* 
        FROM {0}shop{1}
        INNER JOIN {0}area{1}
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
            AND ({0}area{1}.{0}id{1} = @0 OR {0}area{1}.{0}id{1} = @1)"), db.Command.CommandText);

      //InnerJoin Addtional include `OR` left
      select = db.Adapter.CreateSelect();
      select.AddFrom("shop").AddColumns("*");
      select.Context("shop")
        .InnerJoin(
          "area",
          db.Adapter.CreateCondition()
            .Add(
              new Sdx.Db.Sql.Column("area_id", "shop"),
              new Sdx.Db.Sql.Column("id", "area")
            ).Add(
              db.Adapter.CreateCondition()
                .Add(new Sdx.Db.Sql.Column("id", "shop"), "1")
                .AddOr(new Sdx.Db.Sql.Column("id", "shop"), "2")
            )
        )
        .AddColumns("*");

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
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("*")
        .InnerJoin("area", db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("area_id", "shop"),
          new Sdx.Db.Sql.Column("id", "area")
         ))
        .InnerJoin("large_area", db.Adapter.CreateCondition().Add(
          new Sdx.Db.Sql.Column("large_area_id", "area"),
          new Sdx.Db.Sql.Column("id", "large_area")
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

    [Fact]
    public void TestSelectHavingSubquery()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectHavingSubquery(db);
        ExecSql(db);
      }
    }

    private void RunSelectHavingSubquery(TestDb db)
    {
      var sub = db.Adapter.CreateSelect();
      sub
        .AddFrom("shop_category")
        .AddColumn("shop_Id")
        .Where.Add("category_id", 3);

      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .AddGroup("id")
        .Having.Add("id", sub, Sdx.Db.Sql.Comparison.In);

      db.Command = select.Build();
      Assert.Equal(
       db.Sql(@"SELECT
         {0}shop{1}.{0}id{1}
        FROM {0}shop{1} 
        GROUP BY {0}shop{1}.{0}id{1} 
        HAVING {0}shop{1}.{0}id{1} IN (SELECT {0}shop_category{1}.{0}shop_Id{1} FROM {0}shop_category{1} WHERE {0}shop_category{1}.{0}category_id{1} = @0)"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal(3, db.Command.Parameters["@0"].Value);
    }

    [Fact]
    public void TestSelectComment()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectComment(db);
        ExecSql(db);
      }
    }

    private void RunSelectComment(TestDb db)
    {
      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();

      //
      var select = db.Adapter.CreateSelect();
      select
        .SetComment("No where comment")
        .AddFrom(new Test.Orm.Table.Shop())
        .AddColumn("id");

      var profiler = Sdx.Context.Current.DbProfiler;

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var rset = conn.FetchRecordSet(select);

        Assert.Equal(1, profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.Equal("No where comment", profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList()[0].Comment);
      }


      //
      select
        .SetComment("Where and order limit")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(10)
        .Context("shop").Where.Add("area_id", 1);

      using (var conn = db.Adapter.CreateConnection())
      {
        conn.Open();
        var rset = conn.FetchRecordSet(select);
        Assert.Equal(2, profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.Equal("Where and order limit", profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList()[1].Comment);
      }

      //
      select = db.Adapter.CreateSelect();

      select
        .SetComment("No where form string")
        .AddFrom("shop")
        .AddColumn("id");

      var command = select.Build();
      using (var con = db.Adapter.CreateConnection())
      {
        con.Open();
        var reader = con.ExecuteReader(command);
        Assert.Equal(3, profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.Equal("No where form string", profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList()[2].Comment);
      }


      //
      select
        .SetComment("Where and order limit from string")
        .AddOrder("id", Sdx.Db.Sql.Order.ASC)
        .SetLimit(10)
        .Context("shop").Where.Add("area_id", 1);

      command = select.Build();
      using (var con = db.Adapter.CreateConnection())
      {
        con.Open();
        var reader = con.ExecuteReader(command);
        Assert.Equal(4, profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList().Count);
        Assert.Equal("Where and order limit from string", profiler.Logs.Where(log => log.CommandText.StartsWith("SELECT")).ToList()[3].Comment);
      }
    }

    [Fact]
    public void TestSelectBetween()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectBetween(db);
        ExecSql(db);
      }
    }

    private void RunSelectBetween(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .Where.AddBetween(
          "created_at",
          "2015-12-10 00:00:00",
          "2015-12-10 23:59:59"
        );

      db.Command = select.Build();

      Assert.Equal(
        db.Sql(@"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE {0}shop{1}.{0}created_at{1} BETWEEN @0 AND @1"),
        db.Command.CommandText
      );

      Assert.Equal("2015-12-10 00:00:00", db.Command.Parameters["@0"].Value);
      Assert.Equal("2015-12-10 23:59:59", db.Command.Parameters["@1"].Value);

      select.Context("shop")
        .Where.AddBetweenOr(
          "created_at",
          "2015-12-20 00:00:00",
          "2015-12-20 23:59:59"
        );

      db.Command = select.Build();
      Assert.Equal(
        db.Sql(@"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} 
          WHERE {0}shop{1}.{0}created_at{1} BETWEEN @0 AND @1
          OR {0}shop{1}.{0}created_at{1} BETWEEN @2 AND @3"),
        db.Command.CommandText
      );

      Assert.Equal("2015-12-20 00:00:00", db.Command.Parameters["@2"].Value);
      Assert.Equal("2015-12-20 23:59:59", db.Command.Parameters["@3"].Value);
    }

    [Fact]
    public void TestSelectNotBetween()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectNotBetween(db);
        ExecSql(db);
      }
    }

    private void RunSelectNotBetween(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("id")
        .Where.AddNotBetween(
          "created_at",
          "2015-12-10 00:00:00",
          "2015-12-10 23:59:59"
        );

      db.Command = select.Build();

      Assert.Equal(
        db.Sql(@"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE {0}shop{1}.{0}created_at{1} NOT BETWEEN @0 AND @1"),
        db.Command.CommandText
      );

      Assert.Equal("2015-12-10 00:00:00", db.Command.Parameters["@0"].Value);
      Assert.Equal("2015-12-10 23:59:59", db.Command.Parameters["@1"].Value);

      select.Context("shop")
        .Where.AddNotBetween(
          "created_at",
          "2015-12-20 00:00:00",
          "2015-12-20 23:59:59"
        );

      db.Command = select.Build();
      Assert.Equal(
        db.Sql(@"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} 
          WHERE {0}shop{1}.{0}created_at{1} NOT BETWEEN @0 AND @1
          AND {0}shop{1}.{0}created_at{1} NOT BETWEEN @2 AND @3"),
        db.Command.CommandText
      );

      Assert.Equal("2015-12-20 00:00:00", db.Command.Parameters["@2"].Value);
      Assert.Equal("2015-12-20 23:59:59", db.Command.Parameters["@3"].Value);
    }

    [Fact]
    public void TestSelectFree()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectFree(db);
        ExecSql(db);
      }
    }

    private void RunSelectFree(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom("shop")
        .AddColumn("id");

      select.Where.Add(Sdx.Db.Sql.Expr.Wrap("id = 1"));

      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE id = 1"),
        db.Command.CommandText
      );

      select.Where.Add(Sdx.Db.Sql.Expr.Wrap(" /* foo bar */"));
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE id = 1 /* foo bar */"),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestSelectForUpdate()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectForUpdate(db);
        ExecSql(db);
      }
    }

    private void RunSelectForUpdate(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select.ForUpdate = true;

      select
        .AddFrom(new Test.Orm.Table.Shop())
        .AddColumn("id")
        .Where.Add("id", 1)
        ;

      select.Context("shop").Table.SetColumns("id");

      using (var command = select.Build())
      {
        if (db.Adapter is Sdx.Db.Adapter.SqlServer)
        {
          Assert.Equal(db.Sql(@"
SELECT [shop].[id] AS [id@shop] FROM [shop] 
  WITH (UPDLOCK,ROWLOCK) 
  WHERE [shop].[id] = @0"), command.CommandText);
        }
        else if (db.Adapter is Sdx.Db.Adapter.MySql)
        {
          Assert.Equal(db.Sql(@"
SELECT `shop`.`id` AS `id@shop` FROM `shop` 
  WHERE `shop`.`id` = @0 
  FOR UPDATE"), command.CommandText);
        }

      }

      //実行で例外が出ないかチェック。
      using (var con = db.Adapter.CreateConnection())
      {
        con.Open();
        con.BeginTransaction();
        var shop = con.FetchRecord(select);
        con.Commit();
      }
    }

    [Fact]
    public void TestSelectSubqueryColumn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunSelectSubqueryColumn(db);
        ExecSql(db);
      }
    }

    private void RunSelectSubqueryColumn(TestDb testDb)
    {
      Sdx.Db.Sql.Select select = testDb.Adapter.CreateSelect();
      select.AddFrom("shop");
      select.AddColumn(Sdx.Db.Sql.Expr.Wrap("(SELECT id FROM shop sub WHERE shop.id = sub.id)"), "sub_id");

      testDb.Command = select.Build();

      Assert.Equal(testDb.Sql(@"SELECT
      (SELECT id FROM shop sub WHERE shop.id = sub.id) AS {0}sub_id{1}
      FROM {0}shop{1}"), testDb.Command.CommandText);


      select = testDb.Adapter.CreateSelect();
      select.AddFrom("shop");

      var sub = testDb.Adapter.CreateSelect();
      sub
        .AddFrom("shop", "sub")
        .AddColumn("id");
      sub.Where.Add(
        new Sdx.Db.Sql.Column("id", "shop"),
        new Sdx.Db.Sql.Column("id", "sub")
      );

      select.AddColumn(sub, "sub_id");

      testDb.Command = select.Build();

      Assert.Equal(testDb.Sql(@"SELECT
          (SELECT {0}sub{1}.{0}id{1}
            FROM {0}shop{1} AS {0}sub{1}
            WHERE {0}shop{1}.{0}id{1} = {0}sub{1}.{0}id{1}) AS {0}sub_id{1}
        FROM {0}shop{1}"), testDb.Command.CommandText);
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


    [Fact]
    public void TestCountRow()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunCountRow(db);
        ExecSql(db);
      }
    }

    private void RunCountRow(TestDb testDb)
    {
      var db = testDb.Adapter;

      using(var conn = db.CreateConnection())
      {
        conn.Open();

        var select = db.CreateSelect();
        select
          .AddFrom(new Test.Orm.Table.ShopCategory())
          ;

        var scSet = conn.FetchRecordSet(select);
        Assert.Equal(scSet.Count, conn.CountRow(select));

        select = db.CreateSelect();
        select
          .AddFrom(new Test.Orm.Table.Shop())
          .SetColumn("id")
          .AddGroup("id")
          .InnerJoin(new Test.Orm.Table.ShopCategory())
          .ClearColumns()
          ;

        var count = conn.CountRow(select);
        Assert.NotEqual(scSet.Count, count);
        Assert.Equal(scSet.GroupBy(sc => sc.GetValue("shop_id")).Count(), count);
      }
    }

    [Fact]
    public void TestIntegerIn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunIntegerIn(db);
        ExecSql(db);
      }
    }

    private void RunIntegerIn(TestDb testDb)
    {
      var select = testDb.Adapter.CreateSelect();

      select
        .AddFrom(new Test.Orm.Table.Shop())
        .Where.Add("id", new int[]{5,10,15});

      testDb.Command = select.Build();
      Assert.Equal(testDb.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1}, {0}shop{1}.{0}name{1} AS {0}name@shop{1}, {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1}, {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1}, {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1}, {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1}, {0}shop{1}.{0}password{1} AS {0}password@shop{1}, {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1} 
          FROM {0}shop{1} 
          WHERE {0}shop{1}.{0}id{1} IN (@0, @1, @2)
"), testDb.Command.CommandText);

      Assert.Equal(3, testDb.Command.Parameters.Count);
      Assert.Equal(5, testDb.Command.Parameters["@0"].Value);
      Assert.Equal(10, testDb.Command.Parameters["@1"].Value);
      Assert.Equal(15, testDb.Command.Parameters["@2"].Value);
    }

    [Fact]
    public void TestOrderRandom()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunOrderRandom(db);
        ExecSql(db);
      }
    }

    private void RunOrderRandom(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop())
        .SetColumn("id")
        .AddOrderRandom();

      db.Command = select.Build();

      Assert.Equal(
        db.Sql(
          @"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}
          ORDER BY " + db.Adapter.RandomOrderKeyword()
        ),
        db.Command.CommandText
      );
    }

    [Fact]
    public void TestAliasInnerJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunAliasInnerJoin(db);
        ExecSql(db);
      }
    }

    private void RunAliasInnerJoin(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop(), cShop => 
        {
          cShop.InnerJoin(new Test.Orm.Table.Area(), "area_1", cArea => 
          {
            Assert.Equal("area_1", cArea.Name);
            cArea.Where.Add("name", "area_1_name");
          });

          cShop.InnerJoin(new Test.Orm.Table.Area(), "area_2", cArea =>
          {
            Assert.Equal("area_2", cArea.Name);
            cArea.Where.Add("name", "area_2_name");
          });
        });

      Assert.False(select.HasContext("area"));
      Assert.True(select.HasContext("area_1"));
      Assert.True(select.HasContext("area_2"));

      Assert.Equal("area_1", select.Context("area_1").Name);
      Assert.Equal("area_2", select.Context("area_2").Name);

      db.Command = select.Build();

      Assert.Equal(
        db.Sql(
          @"SELECT {0}shop{1}.{0}id{1} AS {0}id@shop{1}, 
            {0}shop{1}.{0}name{1} AS {0}name@shop{1}, 
            {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1}, 
            {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1}, 
            {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1}, 
            {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1}, 
            {0}shop{1}.{0}password{1} AS {0}password@shop{1}, 
            {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1}, 
            {0}area_1{1}.{0}id{1} AS {0}id@area_1{1}, 
            {0}area_1{1}.{0}name{1} AS {0}name@area_1{1}, 
            {0}area_1{1}.{0}code{1} AS {0}code@area_1{1}, 
            {0}area_1{1}.{0}large_area_id{1} AS {0}large_area_id@area_1{1}, 
            {0}area_1{1}.{0}sequence{1} AS {0}sequence@area_1{1}, 
            {0}area_2{1}.{0}id{1} AS {0}id@area_2{1}, 
            {0}area_2{1}.{0}name{1} AS {0}name@area_2{1}, 
            {0}area_2{1}.{0}code{1} AS {0}code@area_2{1}, 
            {0}area_2{1}.{0}large_area_id{1} AS {0}large_area_id@area_2{1}, 
            {0}area_2{1}.{0}sequence{1} AS {0}sequence@area_2{1} 
              FROM {0}shop{1} 
              INNER JOIN {0}area{1} AS {0}area_1{1} ON {0}shop{1}.{0}area_id{1} = {0}area_1{1}.{0}id{1} 
              INNER JOIN {0}area{1} AS {0}area_2{1} ON {0}shop{1}.{0}area_id{1} = {0}area_2{1}.{0}id{1} 
              WHERE {0}area_1{1}.{0}name{1} = @0 AND {0}area_2{1}.{0}name{1} = @1"
        ),
        db.Command.CommandText
      );

      Assert.Equal("area_1_name", db.Command.Parameters["@0"].Value);
      Assert.Equal("area_2_name", db.Command.Parameters["@1"].Value);
    }

    [Fact]
    public void TestAliasLeftJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunAliasLeftJoin(db);
        ExecSql(db);
      }
    }

    private void RunAliasLeftJoin(TestDb db)
    {
      var select = db.Adapter.CreateSelect();
      select
        .AddFrom(new Test.Orm.Table.Shop(), cShop =>
        {
          cShop.LeftJoin(new Test.Orm.Table.Area(), "area_1", cArea =>
          {
            Assert.Equal("area_1", cArea.Name);
            cArea.Where.Add("name", "area_1_name");
          });

          cShop.LeftJoin(new Test.Orm.Table.Area(), "area_2", cArea =>
          {
            Assert.Equal("area_2", cArea.Name);
            cArea.Where.Add("name", "area_2_name");
          });
        });

      Assert.False(select.HasContext("area"));
      Assert.True(select.HasContext("area_1"));
      Assert.True(select.HasContext("area_2"));

      Assert.Equal("area_1", select.Context("area_1").Name);
      Assert.Equal("area_2", select.Context("area_2").Name);

      db.Command = select.Build();

      Assert.Equal(
        db.Sql(
          @"SELECT {0}shop{1}.{0}id{1} AS {0}id@shop{1}, 
        {0}shop{1}.{0}name{1} AS {0}name@shop{1}, 
        {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1}, 
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1}, 
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1}, 
        {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1}, 
        {0}shop{1}.{0}password{1} AS {0}password@shop{1}, 
        {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1}, 
        {0}area_1{1}.{0}id{1} AS {0}id@area_1{1}, 
        {0}area_1{1}.{0}name{1} AS {0}name@area_1{1}, 
        {0}area_1{1}.{0}code{1} AS {0}code@area_1{1}, 
        {0}area_1{1}.{0}large_area_id{1} AS {0}large_area_id@area_1{1}, 
        {0}area_1{1}.{0}sequence{1} AS {0}sequence@area_1{1}, 
        {0}area_2{1}.{0}id{1} AS {0}id@area_2{1}, 
        {0}area_2{1}.{0}name{1} AS {0}name@area_2{1}, 
        {0}area_2{1}.{0}code{1} AS {0}code@area_2{1}, 
        {0}area_2{1}.{0}large_area_id{1} AS {0}large_area_id@area_2{1}, 
        {0}area_2{1}.{0}sequence{1} AS {0}sequence@area_2{1} 
          FROM {0}shop{1} 
          LEFT JOIN {0}area{1} AS {0}area_1{1} ON {0}shop{1}.{0}area_id{1} = {0}area_1{1}.{0}id{1} 
          LEFT JOIN {0}area{1} AS {0}area_2{1} ON {0}shop{1}.{0}area_id{1} = {0}area_2{1}.{0}id{1} 
          WHERE {0}area_1{1}.{0}name{1} = @0 AND {0}area_2{1}.{0}name{1} = @1"
        ),
        db.Command.CommandText
      );

      Assert.Equal("area_1_name", db.Command.Parameters["@0"].Value);
      Assert.Equal("area_2_name", db.Command.Parameters["@1"].Value);
    }

    [Fact]
    public void TestGroupAndOrder()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunGroupAndOrder(db);
        ExecSql(db);
      }
    }

    private void RunGroupAndOrder(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Shop(), cShop => 
      {
        cShop.AddOrder("id", Sdx.Db.Sql.Order.DESC);
        cShop.AddOrder("created_at", Sdx.Db.Sql.Order.DESC);
        cShop.Table.SetColumns("id");
        cShop.AddGroup("id");

        cShop.InnerJoin(new Test.Orm.Table.Area(), cArea =>
        {
          cArea.AddOrder("id", Sdx.Db.Sql.Order.DESC);
          cArea.AddOrder("name", Sdx.Db.Sql.Order.DESC);
          cArea.Table.SetColumns("id");
          cArea.AddGroup("id");
        });
      });

      testDb.Command = select.Build();

      //Group Byに無いカラムは自動的にOrderから取り除かれます。
      //SELECT句はからは取り除きません。DBベンダーによっては取得できますし、意味がないわけではないので。
      Assert.Equal(
        testDb.Sql(
          @"SELECT {0}shop{1}.{0}id{1} AS {0}id@shop{1}, {0}area{1}.{0}id{1} AS {0}id@area{1} 
  FROM {0}shop{1} 
  INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1} 
  GROUP BY {0}shop{1}.{0}id{1}, {0}area{1}.{0}id{1} 
  ORDER BY {0}shop{1}.{0}id{1} DESC, {0}area{1}.{0}id{1} DESC
"
        ),
        testDb.Command.CommandText
      );
    }

    [Fact]
    public void TestAliasRecordCache()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunAliasRecordCache(db);
        ExecSql(db);
      }
    }

    private void RunAliasRecordCache(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop(), cShop =>
      {
        cShop.Where.Add("id", 5);
        cShop.InnerJoin(new Test.Orm.Table.ShopCategory(), "ShopCategory1", cShopCategory => 
        {
          cShopCategory.Where.Add("category_id", 4);
        });
        cShop.InnerJoin(new Test.Orm.Table.ShopCategory(), "ShopCategory2", cShopCategory =>
        {
          cShopCategory.Where.Add("category_id", 5);
        });
      });

      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var shop = conn.FetchRecord(select);

        var sc1 = shop.GetRecordSet("ShopCategory1");
        Assert.Equal(1, sc1.Count);
        Assert.Equal(4, sc1[0].GetInt32("category_id"));

        var sc2 = shop.GetRecordSet("ShopCategory2");
        Assert.Equal(1, sc2.Count);
        Assert.Equal(5, sc2[0].GetInt32("category_id"));
      }
    }

    [Fact]
    public void TestNotIn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunNotIn(db);
        ExecSql(db);
      }
    }

    private void RunNotIn(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop(), cShop =>
      {
        cShop.SetColumns("id");
        cShop.Where.Add("id", new List<int> { 1, 2 }, Sdx.Db.Sql.Comparison.NotIn);
      });

      testDb.Command = select.Build();

      Assert.Equal(
        testDb.Sql(
          @"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} NOT IN (@0, @1)"
        ),
        testDb.Command.CommandText
      );
    }

    [Fact]
    public void TestInEmptyList()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunInEmptyList(db);
        ExecSql(db);
      }
    }

    private void RunInEmptyList(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop(), cShop =>
      {
        cShop.SetColumns("id");
        cShop.Where.Add("id", new List<int>());
      });

      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      using(var conn = db.CreateConnection())
      {
        conn.Open();
        var ids = conn.FetchList<int>(select);
        Assert.Equal(0, ids.Count);
      }

      Assert.Equal(
        testDb.Sql(
          @"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE 'id@shop' IN ('EMPTY')"
        ),
        Sdx.Context.Current.DbProfiler.Logs[1].CommandText
      );
    }

    [Fact]
    public void TestNotInEmptyList()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunNotInEmptyList(db);
        ExecSql(db);
      }
    }

    private void RunNotInEmptyList(TestDb testDb)
    {
      var db = testDb.Adapter;
      var select = db.CreateSelect();

      select.AddFrom(new Test.Orm.Table.Shop(), cShop =>
      {
        cShop.SetColumns("id");
        cShop.Where.Add("id", new List<int>(), Sdx.Db.Sql.Comparison.NotIn);
      });

      Sdx.Context.Current.DbProfiler = new Sdx.Db.Sql.Profiler();
      using (var conn = db.CreateConnection())
      {
        conn.Open();
        var ids = conn.FetchList<int>(select);
        Assert.NotEqual(0, ids.Count);
      }

      Assert.Equal(
        testDb.Sql(
          @"SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} WHERE 'id@shop' NOT IN ('EMPTY')"
        ),
        Sdx.Context.Current.DbProfiler.Logs[1].CommandText
      );
    }
  }
}
