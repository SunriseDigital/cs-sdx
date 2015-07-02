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
    private String MySqlConnectionString
    {
      get { return "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963"; }
    }

    private String SqlServerConnectionString
    {
      get { return "Server=.\\SQLEXPRESS;Database=sdxtest;User Id=sdxuser;Password=sdx5963;"; }
    }

    private void ResetMySqlDatabase()
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

      factory.ConnectionString = this.MySqlConnectionString;
      var con = factory.CreateConnection();
      using(con)
      {
        con.Open();
        this.ExecuteSqlFile(con, "setup.mysql.sql");
        this.ExecuteSqlFile(con, "insert.sql");
      }

      Console.WriteLine("ResetMySqlDatabase");
    }

    [Conditional("ON_VISUAL_STUDIO")]
    private void ResetSqlServerDatabase()
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

      factory.ConnectionString = this.SqlServerConnectionString;
      var con = factory.CreateConnection();
      using(con)
      {
        con.Open();
        this.ExecuteSqlFile(con, "setup.sqlserver.sql");
        this.ExecuteSqlFile(con, "insert.sql");
      }

      Console.WriteLine("ResetSqlServerDatabase");
    }

    private void ExecuteSqlFile(DbConnection con, string dataFilePath)
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

    [Conditional("ON_VISUAL_STUDIO")]
    private void RunFactorySimpleRetrieveForSqlServer()
    {
      ResetSqlServerDatabase();
      var factory = new Sdx.Db.SqlServerFactory();
      factory.ConnectionString = this.SqlServerConnectionString;
      RunFactorySimpleRetrieve(factory);
    }

    private void RunFactorySimpleRetrieveForMySql()
    {
      ResetMySqlDatabase();
      var factory = new Sdx.Db.MySqlFactory();
      factory.ConnectionString = this.MySqlConnectionString;
      RunFactorySimpleRetrieve(factory);
    }

    private void RunFactorySimpleRetrieve(Sdx.Db.Factory factory)
    {
      var con = factory.CreateConnection();
      using (con)
      {
        con.Open();


        DbCommand command = con.CreateCommand();
        command.CommandText = "SELECT shop.name as name_shop, category.name as name_category FROM shop"
          + " INNER JOIN category ON category.id = shop.category_id"
          + " WHERE shop.id = @shop@id"
          ;

        command.Parameters.Add(factory.CreateParameter("@shop@id", "1"));

        DbDataAdapter adapter = factory.CreateDataAdapter();
        DataSet dataset = new DataSet();

        adapter.SelectCommand = command;
        adapter.Fill(dataset);
        foreach (DataRow row in dataset.Tables[0].Rows)
        {
          Console.WriteLine(Sdx.DebugTool.Debug.Dump(row["name_category"]));
        }

        Assert.Equal(1, dataset.Tables[0].Rows.Count);
        Assert.Equal("天祥", dataset.Tables[0].Rows[0]["name_shop"]);
        Assert.Equal("中華", dataset.Tables[0].Rows[0]["name_category"]);
      }
    }

    [Fact]
    public void TestFactorySimpleRetrieve()
    {
      RunFactorySimpleRetrieveForSqlServer();
      RunFactorySimpleRetrieveForMySql();
    }

    [Fact]
    public void TestWhereWithTable()
    {
      var factory = new Sdx.Db.SqlServerFactory();
      Sdx.Db.Where where = factory.CreateWhere();
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
      this.RunWhereSimpleForSqlServer(new Sdx.Db.SqlServerFactory());
      this.RunWhereSimpleForMySql(new Sdx.Db.MySqlFactory());
    }

    private void RunWhereSimpleForSqlServer(Sdx.Db.Factory factory)
    {
      Sdx.Db.Where where = factory.CreateWhere();

      where.add("id", "1");

      Assert.Equal("[id] = '1'", Sdx.Db.Util.CommandToSql(where.build()));

      where.add("type", 2);
      Assert.Equal("[id] = '1' AND [type] = '2'", Sdx.Db.Util.CommandToSql(where.build()));
    }

    private void RunWhereSimpleForMySql(Sdx.Db.Factory factory)
    {
      Sdx.Db.Where where = factory.CreateWhere();

      where.add("id", "1");

      Assert.Equal("`id` = '1'", Sdx.Db.Util.CommandToSql(where.build()));

      where.add("type", 2);
      Assert.Equal("`id` = '1' AND `type` = '2'", Sdx.Db.Util.CommandToSql(where.build()));
    }

    [Fact]
    public void TestSimpleSelect()
    {
      List<DbCommand> commands;
      Sdx.Db.Factory factory;

      factory = new Sdx.Db.SqlServerFactory();
      factory.ConnectionString = this.SqlServerConnectionString;
      commands = this.RunSimpleSelect(factory, "[", "]");
      this.execDbCommandForSqlServer(factory, commands);

      factory = new Sdx.Db.MySqlFactory();
      factory.ConnectionString = this.MySqlConnectionString;
      commands = this.RunSimpleSelect(factory, "`", "`");
      this.execDbCommandForSqlServer(factory, commands);
    }

    private List<DbCommand> RunSimpleSelect(Sdx.Db.Factory factory, String leftQuoteChar, String rightQuoteChar)
    {
      Sdx.Db.Select select = factory.CreateSelect();

      List<DbCommand> commands = new List<DbCommand>();

      select.From("shop");
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}shop{1}.* FROM {0}shop{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      select.From("shop", "s");
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}s{1}.* FROM {0}shop{1} AS {0}s{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      select.From("shop");
      select.Table("shop").Columns.Add("id");
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      select.Table("shop").Columns.Clear();
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}shop{1}.* FROM {0}shop{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      select.Table("shop").SetColumns(new String[] { "id" });
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      select.Table("shop").SetColumns("id");
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}shop{1}.{0}id{1} FROM {0}shop{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      select.Table("shop").AddColumn("name");
      commands.Add(select.build());
      Assert.Equal(
        String.Format("SELECT {0}shop{1}.{0}id{1}, {0}shop{1}.{0}name{1} FROM {0}shop{1}", leftQuoteChar, rightQuoteChar),
        commands[commands.Count - 1].CommandText
      );

      return commands;
    }

    [Conditional("ON_VISUAL_STUDIO")]
    private void execDbCommandForSqlServer(Sdx.Db.Factory factory, List<DbCommand> commands)
    {
      this.execDbCommand(factory, commands);
    }

    private void execDbCommandForMySql(Sdx.Db.Factory factory, List<DbCommand> commands)
    {
      this.execDbCommand(factory, commands);
    }

    /// <summary>
    /// DbCommandを一度実行してみるメソッド。特にAssertはしていません。Syntax errorのチェック用です。
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="commands"></param>
    private void execDbCommand(Sdx.Db.Factory factory, List<DbCommand> commands)
    {
      commands.ForEach(command => {
        DbConnection con = factory.CreateConnection();
        using(con)
        {
          con.Open();
          command.Connection = con;
          DbDataAdapter adapter = factory.CreateDataAdapter();
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
