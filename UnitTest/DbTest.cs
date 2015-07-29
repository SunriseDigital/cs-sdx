﻿using System;
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
  public class DbTest : BaseTest
  {
    /// <summary>
    /// 複数のDBのテストをまとめて行うためのDbFactoryのラッパークラス
    /// CreateTestDbList()メソッドで生成しています。
    /// </summary>
    class TestDb
    {
      private DbCommand command;
      private List<DbCommand> commands = new List<DbCommand>();
      public Sdx.Db.Factory Factory { get; set; }
      public String LeftQuoteChar { get; set; }
      public String RightQupteChar { get; set; }
      public DbCommand Command
      {
        get { return this.command; }
        set { this.command = value; this.commands.Add(command); }
      }
      public List<DbCommand> Commands { get { return this.commands; } }
      public String Sql(String sql)
      {
        return String.Format(sql, this.LeftQuoteChar, this.RightQupteChar);
      }
    }

    [ClassInitialize]
    public static void InitilizeClass(TestContext context)
    {
      ResetMySqlDatabase();
      ResetSqlServerDatabase();
    }

    public override void FixtureSetUp()
    {
      DbTest.InitilizeClass(null);
    }

    private static String MySqlConnectionString
    {
      get { return "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963;"; }
    }

    private static String SqlServerConnectionString
    {
      get { return "Server=.\\SQLEXPRESS;Database=sdxtest;User Id=sdxuser;Password=sdx5963;"; }
    }

    private static void ResetMySqlDatabase()
    {
      Sdx.Db.Factory factory = new Sdx.Db.MySqlFactory();

      var masterCon = factory.CreateConnection();
      masterCon.ConnectionString = "Server=localhost;Database=mysql;Uid=root;Pwd=";
      using (masterCon)
      {
        masterCon.Open();

        //drop and create db
        try
        {
          var dropSql = masterCon.CreateCommand();
          dropSql.CommandText = @"
DROP DATABASE IF EXISTS `sdxtest` ;
CREATE DATABASE `sdxtest` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;
GRANT ALL ON `sdxtest`.* TO 'sdxuser'@'localhost' IDENTIFIED BY 'sdx5963';
";
          dropSql.ExecuteNonQuery();
        }
        catch (DbException e)
        {
          Console.WriteLine(e.Message);
          //do nothing
        }
      }

      factory.ConnectionString = DbTest.MySqlConnectionString;
      var con = factory.CreateConnection();
      using (con)
      {
        con.Open();
        DbTest.ExecuteSqlFile(con, "setup.mysql.sql");
        DbTest.ExecuteSqlFile(con, "insert.sql");
      }

      Console.WriteLine("ResetMySqlDatabase");
    }

    [Conditional("ON_VISUAL_STUDIO")]
    private static void ResetSqlServerDatabase()
    {
      //SdxTestデータベースをDROPします
      Sdx.Db.Factory factory = new Sdx.Db.SqlServerFactory();
      var masterCon = factory.CreateConnection();
      String pwd = ConfigurationManager.AppSettings["SqlServerSaPwd"];
      masterCon.ConnectionString = "Server=.\\SQLEXPRESS;Database=master;User Id=sa;Password=" + pwd;
      using (masterCon)
      {
        masterCon.Open();

        //drop db
        try
        {
          var dropSql = masterCon.CreateCommand();
          dropSql.CommandText = @"
ALTER DATABASE sdxtest SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [sdxtest]
";
          dropSql.ExecuteNonQuery();
        }
        catch (DbException e)
        {
          //do nothing
        }

        //drop user
        try
        {
          var dropUserSql = masterCon.CreateCommand();
          dropUserSql.CommandText = "DROP LOGIN sdxuser";
          dropUserSql.ExecuteNonQuery();
        }
        catch (DbException e)
        {
          //do nothing
        }

        //create db
        var createSql = masterCon.CreateCommand();
        createSql.CommandText = "CREATE DATABASE sdxtest";
        createSql.ExecuteNonQuery();

        //create user
        var createUserSql = masterCon.CreateCommand();
        createUserSql.CommandText = @"
CREATE LOGIN sdxuser WITH PASSWORD = 'sdx5963';
ALTER AUTHORIZATION ON DATABASE::sdxtest TO sdxuser;
";
        createUserSql.ExecuteNonQuery();
      }

      factory.ConnectionString = DbTest.SqlServerConnectionString;
      var con = factory.CreateConnection();
      using (con)
      {
        con.Open();
        DbTest.ExecuteSqlFile(con, "setup.sqlserver.sql");
        DbTest.ExecuteSqlFile(con, "insert.sql");
      }

      Console.WriteLine("ResetSqlServerDatabase");
    }

    private static void ExecuteSqlFile(DbConnection con, string dataFilePath)
    {
      //setup.sqlを流し込みます。
      using (StreamReader stream = new StreamReader(dataFilePath, Encoding.GetEncoding("UTF-8")))
      {
        String setupSql = stream.ReadToEnd();
        DbTransaction sqlTran = con.BeginTransaction();
        DbCommand command = con.CreateCommand();
        command.Transaction = sqlTran;

        try
        {
          // Execute two separate commands.
          command.CommandText = setupSql;
          command.ExecuteNonQuery();

          // Commit the transaction.
          sqlTran.Commit();
        }
        catch (Exception ex)
        {
          sqlTran.Rollback();
          throw ex;
        }
      }
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
      var con = db.Factory.CreateConnection();
      using (con)
      {
        con.Open();


        DbCommand command = con.CreateCommand();
        command.CommandText = "SELECT shop.name as name_shop, category.name as name_category FROM shop"
          + " INNER JOIN category ON category.id = shop.category_id"
          + " WHERE shop.id = @shop@id"
          ;

        command.Parameters.Add(db.Factory.CreateParameter("@shop@id", "1"));

        DbDataAdapter adapter = db.Factory.CreateDataAdapter();
        DataSet dataset = new DataSet();

        adapter.SelectCommand = command;
        adapter.Fill(dataset);

        Assert.Equal(1, dataset.Tables[0].Rows.Count);
        Assert.Equal("天祥", dataset.Tables[0].Rows[0]["name_shop"]);
        Assert.Equal("中華", dataset.Tables[0].Rows[0]["name_category"]);
      }
    }

    [Fact]
    public void TestUtilCommandToSql()
    {
      SqlCommand cmd;

      cmd = new SqlCommand("SELECT * FROM user WHERE city = @City");
      cmd.Parameters.AddWithValue("@City", "東京");
      Assert.Equal("SELECT * FROM user WHERE city = '東京'", Sdx.Db.Util.CommandToSql(cmd));

      cmd = new SqlCommand("SELECT * FROM user WHERE city = @City AND city_code = @CityCode");
      cmd.Parameters.AddWithValue("@City", "東京");
      cmd.Parameters.AddWithValue("@CityCode", "tokyo");
      Assert.Equal("SELECT * FROM user WHERE city = '東京' AND city_code = 'tokyo'", Sdx.Db.Util.CommandToSql(cmd));
    }

    [Fact]
    public void TestWhereSimple()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        var command = db.Factory.CreateCommand();
        var counter = new Sdx.Db.Query.Counter();
        var select = db.Factory.CreateSelect();
        var where = select.CreateWhere();

        where.Add("id", "1");
        command.CommandText = where.Build(command.Parameters, counter);
        Assert.Equal(
          db.Sql("{0}id{1} = '1'"),
          Sdx.Db.Util.CommandToSql(command)
        );

        where.Add("type", 2);
        command.CommandText = where.Build(command.Parameters, counter);
        Assert.Equal(
          db.Sql("{0}id{1} = '1' AND {0}type{1} = '2'"),
          Sdx.Db.Util.CommandToSql(command)
        );
      }
    }

    private List<TestDb> CreateTestDbList()
    {
      var list = new List<TestDb>();
      TestDb testDb;

#if ON_VISUAL_STUDIO
      testDb = new TestDb();
      testDb.Factory = new Sdx.Db.SqlServerFactory();
      testDb.Factory.ConnectionString = DbTest.SqlServerConnectionString;
      testDb.LeftQuoteChar = "[";
      testDb.RightQupteChar = "]";
      list.Add(testDb);
#endif

      testDb = new TestDb();
      testDb.Factory = new Sdx.Db.MySqlFactory();
      testDb.Factory.ConnectionString = DbTest.MySqlConnectionString;
      testDb.LeftQuoteChar = "`";
      testDb.RightQupteChar = "`";
      list.Add(testDb);

      return list;
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();

      //AddColumn
      select.From("shop").AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Remove("shop").From("shop", "s").AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}s{1}.* FROM {0}shop{1} AS {0}s{1}"),
        db.Command.CommandText
      );

      select.Remove("s").From("shop");
      select.Table("shop").AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //SetColumns
      select.Remove("shop").From("shop").SetColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Remove("shop").From("shop").SetColumns("id", "name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Remove("shop").From("shop").SetColumns(new String[] { "id", "name" });
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //AddColumns
      select.Remove("shop").From("shop").AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Table("shop").AddColumns("name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Remove("shop").From("shop").AddColumns("id", "name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Remove("shop").From("shop").AddColumns(new String[] { "id", "name" });
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //ClearColumns
      select.Remove("shop").From("shop").SetColumns(new String[] { "id", "name" });
      select.Table("shop").ClearColumns().AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Table("shop").ClearColumns().AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Table("shop").ClearColumns().AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.Table("shop").AddColumns("name");
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();

      select.From("shop").AddColumns("*");

      select.Table("shop").InnerJoin(
        "category",
        "{0}.category_id = {1}.id"
      ).AddColumns("*");

      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}category{1}.* FROM {0}shop{1} INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id"),
        db.Command.CommandText
      );

      //上書きなので順番が入れ替わるはず
      select.Table("shop").InnerJoin("image", "{0}.main_image_id = {1}.id");

      //同じテーブルをJOINしてもAliasを与えなければ上書きになる
      select.Table("shop").InnerJoin(
        "category",
        "{0}.category_id = {1}.id AND {1}.id = 1"
      ).AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}category{1}.* FROM {0}shop{1} INNER JOIN {0}image{1} ON {0}shop{1}.main_image_id = {0}image{1}.id INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id AND {0}category{1}.id = 1"),
        db.Command.CommandText
      );
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .InnerJoin("category", "{0}.category_id = {1}.id")
        .InnerJoin("category_type", "{0}.category_type_id = {1}.id");

      select.Table("shop").AddColumns("*");

      db.Command = select.Build();

      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id INNER JOIN {0}category_type{1} ON {0}category{1}.category_type_id = {0}category_type{1}.id"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .InnerJoin("category", "{0}.category_id = {1}.id")
        .InnerJoin("category_type", "{0}.category_type_id = {1}.id");

      db.Command = select.Build();

      Assert.Equal(
        db.Sql("SELECT  FROM {0}shop{1} INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id INNER JOIN {0}category_type{1} ON {0}category{1}.category_type_id = {0}category_type{1}.id"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select.From("shop").AddColumns("*");

      select.Table("shop")
        .InnerJoin("image", "{0}.main_image_id = {1}.id", "main_image")
        .AddColumns("*");

      select.Table("shop")
        .InnerJoin("image", "{0}.sub_image_id = {1}.id", "sub_image")
        .AddColumns("*");

      db.Command = select.Build();

      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}main_image{1}.*, {0}sub_image{1}.* FROM {0}shop{1} INNER JOIN {0}image{1} AS {0}main_image{1} ON {0}shop{1}.main_image_id = {0}main_image{1}.id INNER JOIN {0}image{1} AS {0}sub_image{1} ON {0}shop{1}.sub_image_id = {0}sub_image{1}.id"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("id", "shop_id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} AS {0}shop_id{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      select.Table("shop")
        .ClearColumns()
        .AddColumns(new Dictionary<string, object>()
         { 
           {"shop_id", "id"},
           {"shop_name", "name"},
         });

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} AS {0}shop_id{1}, {0}shop{1}.{0}name{1} AS {0}shop_name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      select.Table("shop")
        .ClearColumns()
        .AddColumns(new Dictionary<string, object>()
         { 
           {"shop_id", "id"},
           {"shop_name", Sdx.Db.Query.Expr.Wrap("name")},
         });

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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select.From("shop")
        .AddColumn("*")
        .LeftJoin("image", "{0}.main_image_id={1}.id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} LEFT JOIN {0}image{1} ON {0}shop{1}.main_image_id={0}image{1}.id"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image1");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image2");
      select.Table("shop").InnerJoin("image", "{0}.main_image_id={1}.id", "image3");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image4");
      select.Table("shop").InnerJoin("image", "{0}.main_image_id={1}.id", "image5");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image6");
      select.Table("shop").InnerJoin("image", "{0}.main_image_id={1}.id", "image7");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN {0}image{1} AS {0}image3{1} ON {0}shop{1}.main_image_id={0}image3{1}.id INNER JOIN {0}image{1} AS {0}image5{1} ON {0}shop{1}.main_image_id={0}image5{1}.id INNER JOIN {0}image{1} AS {0}image7{1} ON {0}shop{1}.main_image_id={0}image7{1}.id LEFT JOIN {0}image{1} AS {0}image1{1} ON {0}shop{1}.main_image_id={0}image1{1}.id LEFT JOIN {0}image{1} AS {0}image2{1} ON {0}shop{1}.main_image_id={0}image2{1}.id LEFT JOIN {0}image{1} AS {0}image4{1} ON {0}shop{1}.main_image_id={0}image4{1}.id LEFT JOIN {0}image{1} AS {0}image6{1} ON {0}shop{1}.main_image_id={0}image6{1}.id"),
       db.Command.CommandText
      );

      select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image1");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image2");
      select.Table("shop").InnerJoin("image", "{0}.main_image_id={1}.id", "image3");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image4");
      select.Table("shop").InnerJoin("image", "{0}.main_image_id={1}.id", "image5");
      select.Table("shop").LeftJoin("image", "{0}.main_image_id={1}.id", "image6");
      select.Table("shop").InnerJoin("image", "{0}.main_image_id={1}.id", "image7");

      select.JoinOrder = Sdx.Db.Query.JoinOrder.Natural;

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} LEFT JOIN {0}image{1} AS {0}image1{1} ON {0}shop{1}.main_image_id={0}image1{1}.id LEFT JOIN {0}image{1} AS {0}image2{1} ON {0}shop{1}.main_image_id={0}image2{1}.id INNER JOIN {0}image{1} AS {0}image3{1} ON {0}shop{1}.main_image_id={0}image3{1}.id LEFT JOIN {0}image{1} AS {0}image4{1} ON {0}shop{1}.main_image_id={0}image4{1}.id INNER JOIN {0}image{1} AS {0}image5{1} ON {0}shop{1}.main_image_id={0}image5{1}.id LEFT JOIN {0}image{1} AS {0}image6{1} ON {0}shop{1}.main_image_id={0}image6{1}.id INNER JOIN {0}image{1} AS {0}image7{1} ON {0}shop{1}.main_image_id={0}image7{1}.id"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();

      //単純なAddColumns
      select.From("shop");
      select.AddColumns("id", "name");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1}, {0}name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //テーブル名だけすり替える
      select.Remove("shop").From("category");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1}, {0}name{1} FROM {0}category{1}"),
       db.Command.CommandText
      );

      //SetColumns
      select.Remove("category").From("shop");
      select.SetColumns("name", "category_id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}name{1}, {0}category_id{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //AddColumn Dictinary
      select.ClearColumns().Remove("shop").From("shop");
      select.AddColumns(new Dictionary<string, object>() { 
        {"shop_id", "id"},
        {"shop_name", "name"}
      });

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}id{1} AS {0}shop_id{1}, {0}name{1} AS {0}shop_name{1} FROM {0}shop{1}"),
       db.Command.CommandText
      );

      //AddColumn MAX
      select.ClearColumns().Remove("shop").From("shop");
      select.AddColumn(
        Sdx.Db.Query.Expr.Wrap("MAX(" + select.Table("shop").AppendAlias("id") + ")"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");
      select.From("category");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}, {0}category{1}"),
       db.Command.CommandText
      );
    }

    [Fact]
    public void trySqlAction()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        var con = db.Factory.CreateConnection();
        var command = db.Factory.CreateCommand();
        command.CommandText = db.Sql("SELECT * FROM shop WHERE {0}category_id{1} IN (SELECT id FROM category WHERE id = 1)");
        using (con)
        {
          con.Open();
          command.Connection = con;
          DbDataAdapter adapter = db.Factory.CreateDataAdapter();
          DataSet dataset = new DataSet();
          adapter.SelectCommand = command;
          adapter.Fill(dataset);

          foreach (DataRow row in dataset.Tables[0].Rows)
          {
            Console.WriteLine(Sdx.DebugTool.Debug.Dump(Sdx.Db.Util.ToDictionary(row)));
          }
        }
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
      select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");

      select.Where.Add("id", "1");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);


      //tableに対する呼び出し
      select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");

      select.Table("shop").Where.Add("id", "1");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} = @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters[0].Value);

      //WhereのAdd
      select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");

      select.Where.Add(
        select.CreateWhere()
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
      select = db.Factory.CreateSelect();
      select.From("shop").AddColumn("*");

      select.Where
        .Add(
          select.CreateWhere()
            .Add("id", "3")
            .Add("id", "4")
        ).AddOr(
          select.CreateWhere()
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("*")
        .InnerJoin(
          select.Expr("(SELECT id FROM category WHERE id = 1)"),
          "{0}.category_id = {1}.id",
          "sub_cat"
        );

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN (SELECT id FROM category WHERE id = 1) AS {0}sub_cat{1} ON {0}shop{1}.category_id = {0}sub_cat{1}.id"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = db.Factory.CreateSelect();
      sub
        .From("category")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Table("shop").InnerJoin(sub, "{0}.category_id = {1}.id", "sub_cat");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} INNER JOIN (SELECT {0}category{1}.{0}id{1} FROM {0}category{1} WHERE {0}category{1}.{0}id{1} = @0) AS {0}sub_cat{1} ON {0}shop{1}.category_id = {0}sub_cat{1}.id WHERE {0}shop{1}.{0}id{1} = @1"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = db.Factory.CreateSelect();
      sub
        .From("category")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.Table("shop").Where.Add("category_id", sub, Sdx.Db.Query.Comparison.In);

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} WHERE {0}shop{1}.{0}id{1} = @0 AND {0}shop{1}.{0}category_id{1} IN (SELECT {0}category{1}.{0}id{1} FROM {0}category{1} WHERE {0}category{1}.{0}id{1} = @1)"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      select.From(
        Sdx.Db.Query.Expr.Wrap("(SELECT id FROM category WHERE id = 1)"),
        "sub_cat"
      );

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}, (SELECT id FROM category WHERE id = 1) AS {0}sub_cat{1} WHERE {0}shop{1}.{0}id{1} = @0"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("*")
        .Where.Add("id", "1");

      Sdx.Db.Query.Select sub = db.Factory.CreateSelect();
      sub
        .From("category")
        .AddColumn("id")
        .Where.Add("id", "2");

      select.From(sub, "sub_cat");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}, (SELECT {0}category{1}.{0}id{1} FROM {0}category{1} WHERE {0}category{1}.{0}id{1} = @0) AS {0}sub_cat{1} WHERE {0}shop{1}.{0}id{1} = @1"),
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("id")
        .Group("id");

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1}"),
       db.Command.CommandText
      );

      select.Having.Add(
        Sdx.Db.Query.Expr.Wrap("SUM({shop}.id)"),
        10,
        Sdx.Db.Query.Comparison.GreaterEqual
      );

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1} GROUP BY {0}shop{1}.{0}id{1} HAVING SUM({0}shop{1}.id) >= @0"),
       db.Command.CommandText
      );

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("10", db.Command.Parameters[0].Value);

      //selectに直接
      select = db.Factory.CreateSelect();
      select.From("shop");

      select
        .AddColumn("id")
        .Group("id");

      select.Having.Add(
        Sdx.Db.Query.Expr.Wrap("SUM(id)"),
        20,
        Sdx.Db.Query.Comparison.GreaterEqual
      );

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
      Sdx.Db.Query.Select select = db.Factory.CreateSelect();
      select
        .From("shop")
        .AddColumn("*");

      select.Order("id", Sdx.Db.Query.Order.DESC);

      db.Command = select.Build();
      Assert.Equal(
       db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC"),
       db.Command.CommandText
      );

      select.Limit(100);
      db.Command = select.Build();

      this.AssertCommandText(
        typeof(Sdx.Db.SqlServerFactory),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY"),
        db
      );

      this.AssertCommandText(
        typeof(Sdx.Db.MySqlFactory),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC LIMIT 100"),
        db
      );

      select.Limit(100, 10);
      db.Command = select.Build();

      this.AssertCommandText(
        typeof(Sdx.Db.SqlServerFactory),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC OFFSET 10 ROWS FETCH NEXT 100 ROWS ONLY"),
        db
      );

      this.AssertCommandText(
        typeof(Sdx.Db.MySqlFactory),
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1} ORDER BY {0}id{1} DESC LIMIT 100 OFFSET 10"),
        db
      );
      
    }

    private void AssertCommandText(Type type, string expected, TestDb db)
    {
      Console.WriteLine(type);
      if (type != db.Factory.GetType())
      {
        return;
      }

      Assert.Equal(expected, db.Command.CommandText);
    }




    /// <summary>
    /// DbCommandを一度実行してみるメソッド。特にAssertはしていません。Syntax errorのチェック用です。
    /// </summary>
    /// <param name="select"></param>
    /// <param name="commands"></param>
    private void ExecSql(TestDb db)
    {
      db.Commands.ForEach(command =>
      {
        DbConnection con = db.Factory.CreateConnection();
        using (con)
        {
          con.Open();
          command.Connection = con;
          DbDataAdapter adapter = db.Factory.CreateDataAdapter();
          DataSet dataset = new DataSet();
          adapter.SelectCommand = command;
          adapter.Fill(dataset);

          Console.WriteLine("execDbCommand");
          foreach (DataRow row in dataset.Tables[0].Rows)
          {
            Console.WriteLine(Sdx.DebugTool.Debug.Dump(Sdx.Db.Util.ToDictionary(row)));
          }
        }
      });
    }
  }
}