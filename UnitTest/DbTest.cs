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
  public class DbTest :BaseTest
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
      get { return "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963"; }
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
      using(con)
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
        catch(DbException e)
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
      using(con)
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
    public void TestWhereWithTable()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        Sdx.Db.Where where = db.Factory.CreateWhere();
        where.add("id", "1", "shop");

        Assert.Equal(
          db.Sql("{0}shop{1}.{0}id{1} = '1'"),
          Sdx.Db.Util.CommandToSql(where.build())
        );

        where.add("type", 2, "category");
        Assert.Equal(
          db.Sql("{0}shop{1}.{0}id{1} = '1' AND {0}category{1}.{0}type{1} = '2'"),
          Sdx.Db.Util.CommandToSql(where.build())
        );
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
        Sdx.Db.Where where = db.Factory.CreateWhere();

        where.add("id", "1");

        Assert.Equal(
          db.Sql("{0}id{1} = '1'"),
          Sdx.Db.Util.CommandToSql(where.build())
        );

        where.add("type", 2);
        Assert.Equal(
          db.Sql("{0}id{1} = '1' AND {0}type{1} = '2'"),
          Sdx.Db.Util.CommandToSql(where.build())
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
      foreach(TestDb db in this.CreateTestDbList())
      {
        RunSelectSimple(db);
        ExecSql(db);
      }
    }

    private void RunSelectSimple(TestDb db)
    {
      Sdx.Db.Select select = db.Factory.CreateSelect();

      //AddColumn
      select.From("shop").AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.* FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.From("shop", "s").AddColumns("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}s{1}.* FROM {0}shop{1} AS {0}s{1}"),
        db.Command.CommandText
      );

      select.From("shop");
      select.Table("shop").AddColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //SetColumns
      select.From("shop").SetColumns("id");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.From("shop").SetColumns("id", "name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.From("shop").SetColumns(new String[]{"id", "name"});
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //AddColumns
      select.From("shop").AddColumns("id");
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

      select.From("shop").AddColumns("id", "name");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      select.From("shop").AddColumns(new String[]{"id", "name"});
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}"),
        db.Command.CommandText
      );

      //ClearColumns
      select.From("shop").SetColumns(new String[] { "id", "name" });
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

      select.Table("shop").Columns.Add("name");
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
      Sdx.Db.Select select = db.Factory.CreateSelect();

      select.From("shop").Columns.Add("*");

      select.Table("shop").InnerJoin(
        "category",
        "{0}.category_id = {1}.id"
      ).Columns.Add("*");

      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}category{1}.* FROM {0}shop{1} INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id"),
        db.Command.CommandText
      );

      //同じテーブルをJOINしてもAliasを与えなければ上書きになる
      select.Table("shop").InnerJoin(
        "category",
        "{0}.category_id = {1}.id AND {1}.id = 1"
      ).Columns.Add("*");
      db.Command = select.Build();
      Assert.Equal(
        db.Sql("SELECT {0}shop{1}.*, {0}category{1}.* FROM {0}shop{1} INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id AND {0}category{1}.id = 1"),
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
      Sdx.Db.Select select = db.Factory.CreateSelect();
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

    /// <summary>
    /// DbCommandを一度実行してみるメソッド。特にAssertはしていません。Syntax errorのチェック用です。
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="commands"></param>
    private void ExecSql(TestDb db)
    {
      db.Commands.ForEach(command =>
      {
        DbConnection con = db.Factory.CreateConnection();
        using(con)
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
