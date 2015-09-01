using Xunit;
using UnitTest.DummyClasses;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Data.Common;
using System.Text;
using System.Collections.Generic;
using System.Data;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;
using System.Text.RegularExpressions;

namespace UnitTest
{
  [TestClass]
  public class BaseDbTest : BaseTest
  {
    /// <summary>
    /// 複数のDBのテストをまとめて行うためのDbFactoryのラッパークラス
    /// CreateTestDbList()メソッドで生成しています。
    /// </summary>
    protected class TestDb
    {
      private DbCommand command;
      private List<DbCommand> commands = new List<DbCommand>();
      public Sdx.Db.Adapter Adapter { get; set; }
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
        //改行を取り除く
        sql = sql.Replace(Environment.NewLine, "");

        //連続したスペースを一個のスペースに置き換える
        Regex re = new Regex(" +", RegexOptions.Singleline);
        sql = re.Replace(sql, " ");

        return String.Format(sql, this.LeftQuoteChar, this.RightQupteChar);
      }
    }

    protected List<TestDb> CreateTestDbList()
    {
      var list = new List<TestDb>();
      TestDb testDb;

#if ON_VISUAL_STUDIO
      testDb = new TestDb();
      testDb.Adapter = new Sdx.Db.SqlServerAdapter();
      testDb.Adapter.Profiler = new Sdx.Db.Query.Profiler();
      testDb.Adapter.ConnectionString = DbQueryTest.SqlServerConnectionString;
      testDb.LeftQuoteChar = "[";
      testDb.RightQupteChar = "]";
      list.Add(testDb);
#endif

      testDb = new TestDb();
      testDb.Adapter = new Sdx.Db.MySqlAdapter();
      testDb.Adapter.Profiler = new Sdx.Db.Query.Profiler();
      testDb.Adapter.ConnectionString = DbQueryTest.MySqlConnectionString;
      testDb.LeftQuoteChar = "`";
      testDb.RightQupteChar = "`";
      list.Add(testDb);

      return list;
    }

    public static void InitilizeClass(TestContext context)
    {
      ResetMySqlDatabase();
      ResetSqlServerDatabase();
    }

    public override void FixtureSetUp()
    {
      BaseDbTest.InitilizeClass(null);
    }

    protected static String MySqlConnectionString
    {
      get { return "Server=localhost;Database=sdxtest;Uid=sdxuser;Pwd=sdx5963;"; }
    }

    protected static String SqlServerConnectionString
    {
      get { return "Server=.\\SQLEXPRESS;Database=sdxtest;User Id=sdxuser;Password=sdx5963;"; }
    }

    private static void ResetMySqlDatabase()
    {
      Sdx.Db.Adapter factory = new Sdx.Db.MySqlAdapter();

      var masterCon = factory.CreateConnection();
      String pwd = "";
#if ON_VISUAL_STUDIO
      pwd = ConfigurationManager.AppSettings["MySqlRootPwd"];
#endif
      masterCon.ConnectionString = "Server=localhost;Database=mysql;Uid=root;Pwd=" + pwd;
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

      factory.ConnectionString = BaseDbTest.MySqlConnectionString;
      var con = factory.CreateConnection();
      using (con)
      {
        con.Open();
        BaseDbTest.ExecuteSqlFile(con, "setup.mysql.sql");
        BaseDbTest.ExecuteSqlFile(con, "insert.sql");
      }

      Console.WriteLine("ResetMySqlDatabase");
    }

    [Conditional("ON_VISUAL_STUDIO")]
    private static void ResetSqlServerDatabase()
    {
      //SdxTestデータベースをDROPします
      Sdx.Db.Adapter factory = new Sdx.Db.SqlServerAdapter();
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
          Console.WriteLine(e.Message);
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
          Console.WriteLine(e.Message);
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

      factory.ConnectionString = BaseDbTest.SqlServerConnectionString;
      var con = factory.CreateConnection();
      using (con)
      {
        con.Open();
        BaseDbTest.ExecuteSqlFile(con, "setup.sqlserver.sql");
        BaseDbTest.ExecuteSqlFile(con, "insert.sql");
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

    /// <summary>
    /// DbCommandを一度実行してみるメソッド。特にAssertはしていません。Syntax errorのチェック用です。
    /// </summary>
    /// <param name="select"></param>
    /// <param name="commands"></param>
    protected void ExecSql(TestDb db)
    {
      db.Commands.ForEach(command =>
      {
        this.ExecCommand(command, db.Adapter);
      });
    }

    protected void ExecCommand(DbCommand command, Sdx.Db.Adapter adapter)
    {
      using (var con = adapter.CreateConnection())
      {
        command.Connection = con;
        con.Open();
        
        Console.WriteLine("execDbCommand");
        var reader = adapter.ExecuteReader(command);
        while (reader.Read())
        {
          var row = new Dictionary<string, object>();
          for (var i = 0; i < reader.FieldCount; i++)
          {
            row[reader.GetName(i)] = reader.GetValue(i);
          }

          Console.WriteLine(Sdx.Diagnostics.Debug.Dump(row));
        }
      }
    }
  }
}
