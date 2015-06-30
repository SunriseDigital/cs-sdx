﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;

using Xunit;
using UnitTest.Attibute;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace UnitTest
{
  [TestClass]
  public class DbTest
  {
    public void ResetMySqlDatabase()
    {
      var masterDb = new Sdx.Db.MySqlAdapter();
      masterDb.ConnectionString = "Server=localhost;Database=mysql;Uid=root;Pwd=";
      using (masterDb)
      {
        masterDb.Open();

        //drop and create db
        try
        {
          var dropSql = masterDb.CreateCommand();
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

      var db = this.CreateMySqlConnection();
      using(db)
      {
        db.Open();
        this.ExecuteSqlFile(db, "setup.mysql.sql");
        this.ExecuteSqlFile(db, "insert.sql");
      }

      Console.WriteLine("ResetMySqlDatabase");
    }

    [Conditional("ON_VISUAL_STUDIO")]
    public void ResetSqlDatabase()
    {
      String pwd = ConfigurationManager.AppSettings["SqlServerSaPwd"];
      //SdxTestデータベースをDROPします
      var masterDb = new Sdx.Db.SqlAdapter();
      masterDb.ConnectionString = "Server=.\\SQLEXPRESS;Database=master;User Id=sa;Password=" + pwd;
      using (masterDb)
      {
        masterDb.Open();

        //drop db
        try
        {
          var dropSql = masterDb.CreateCommand();
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
          var dropUserSql = masterDb.CreateCommand();
          dropUserSql.CommandText = "DROP LOGIN sdxuser";
          dropUserSql.ExecuteNonQuery();
        }
        catch (DbException e)
        {
          //do nothing
        }

        //create db
        var createSql = masterDb.CreateCommand();
        createSql.CommandText = "CREATE DATABASE sdxtest";
        createSql.ExecuteNonQuery();

        //create user
        var createUserSql = masterDb.CreateCommand();
        createUserSql.CommandText = @"
CREATE LOGIN sdxuser WITH PASSWORD = 'sdx5963';
ALTER AUTHORIZATION ON DATABASE::sdxtest TO sdxuser;
";
        createUserSql.ExecuteNonQuery();
      }

      var db = CreateSqlConnection();
      using(db)
      {
        db.Open();
        this.ExecuteSqlFile(db, "setup.sqlserver.sql");
        this.ExecuteSqlFile(db, "insert.sql");
      }

      Console.WriteLine("ResetSqlServerDatabase");
    }

    private void ExecuteSqlFile(Sdx.Db.Adapter db, string dataFilePath)
    {
      //setup.sqlを流し込みます。
      using (StreamReader stream = new StreamReader(dataFilePath, Encoding.GetEncoding("UTF-8")))
      {
        String setupSql = stream.ReadToEnd();
        DbTransaction sqlTran = db.BeginTransaction();
        DbCommand command = db.CreateCommand();
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

    private Sdx.Db.Adapter CreateSqlConnection()
    {
      var db = new Sdx.Db.SqlAdapter();
      db.ConnectionString = "Server=.\\SQLEXPRESS;Database=sdxtest;User Id=sdxuser;Password=sdx5963;";
      return db;
    }

    private Sdx.Db.Adapter CreateMySqlConnection()
    {
      var db = new Sdx.Db.MySqlAdapter();
      db.ConnectionString = "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963";
      return db;
    }


    [Conditional("ON_VISUAL_STUDIO")]
    private void ExecuteSqlRetrieve()
    {
      ResetSqlDatabase();
      ExecuteRetrieve(this.CreateSqlConnection());
    }

    private void ExecuteMySqlRetrieve()
    {
      ResetMySqlDatabase();
      ExecuteRetrieve(this.CreateMySqlConnection());
    }

    private void ExecuteRetrieve(Sdx.Db.Adapter db)
    {
      using (db)
      {
        DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        DbCommandBuilder commandBuilder = factory.CreateCommandBuilder();
        db.Open();


        DbCommand command = db.CreateCommand();
        command.CommandText = "SELECT shop.name as name_shop, category.name as name_category FROM shop"
          + " INNER JOIN category ON category.id = shop.category_id"
          + " WHERE shop.id = @shop@id"
          ;

        command.Parameters.Add(db.CreateParameter("@shop@id", "1"));

        DbDataReader reader = command.ExecuteReader();
        List<Dictionary<string, string>> list = Sdx.Db.Util.CreateDictinaryList(reader);
        Console.WriteLine(Sdx.DebugTool.Debug.Dump(list));

        Assert.Equal(1, list.Count());
        Assert.Equal("天祥", list[0]["name_shop"]);
        Assert.Equal("中華", list[0]["name_category"]);
      }
    }

    [Fact]
    public void TestAdapterSimpleRetrieve()
    {
      ExecuteSqlRetrieve();
      ExecuteMySqlRetrieve();
    }

    [Fact]
    public void TestWhereWithTable()
    {
      Sdx.Db.Where where = new Sdx.Db.Where();
      where.add("id", "1", "shop");

      Assert.Equal("[shop].[id] = '1'", Sdx.Db.Util.CommandToSql(where.build()));

      where.add("type", 2, "category");
      Assert.Equal("[shop].[id] = '1' AND [category].[type] = '2'", Sdx.Db.Util.CommandToSql(where.build()));
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
      Sdx.Db.Where where = new Sdx.Db.Where();

      where.add("id", "1");

      Assert.Equal("[id] = '1'", Sdx.Db.Util.CommandToSql(where.build()));

      where.add("type", 2);
      Assert.Equal("[id] = '1' AND [type] = '2'", Sdx.Db.Util.CommandToSql(where.build()));

      DbProviderFactory factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
      where.CommandBuilder = factory.CreateCommandBuilder();
      Assert.Equal("`id` = '1' AND `type` = '2'", Sdx.Db.Util.CommandToSql(where.build()));
    }

    [Fact]
    public void TestAdapterCreate()
    {
      this.RunAdapterCreate(new Sdx.Db.MySqlAdapter());
      this.RunAdapterCreate(new Sdx.Db.SqlAdapter());
    }

    public void RunAdapterCreate(Sdx.Db.Adapter db)
    {
      DbCommand command = db.CreateCommand();

      command.CommandText = "SELECT * FROM shop WHERE id = @id";
      command.Parameters.Add(db.CreateParameter("@id", "1"));

      Assert.Equal(
        "SELECT * FROM shop WHERE id = '1'",
        Sdx.Db.Util.CommandToSql(command)
      );
    }
  }
}
