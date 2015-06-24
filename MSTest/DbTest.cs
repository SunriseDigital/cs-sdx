#if DEBUG
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Data.Common;
using Sdx.Db;
using Assert = Xunit.Assert;

namespace MSTest
{
  [TestClass]
  public class DbTest
  {
    [TestInitialize]
    public void setup()
    {
      using (StreamReader stream = new StreamReader("setup.sql", Encoding.GetEncoding("UTF-8")))
      {
        String setupSql = stream.ReadToEnd();
        using (SqlConnection connection = CreateConnection())
        {
          connection.Open();
          SqlTransaction sqlTran = connection.BeginTransaction();
          SqlCommand command = connection.CreateCommand();
          command.Transaction = sqlTran;

          try
          {
            // Execute two separate commands.
            command.CommandText = setupSql;
            command.ExecuteNonQuery();

            // Commit the transaction.
            sqlTran.Commit();
            Console.WriteLine("Executed");
          }
          catch (Exception ex)
          {
            sqlTran.Rollback();
            throw ex;
          }
        }
      }
    }

    private SqlConnection CreateConnection()
    {
      String connectionString = "Server=.\\SQLEXPRESS;Database=SdxTest;User Id=sdxtest;Password=sdx5963;";
      return new SqlConnection(connectionString);
    }

    [TestMethod]
    public void TestMethod1()
    {

      using (SqlConnection connection = CreateConnection())
      {
        DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        DbCommandBuilder commandBuilder = factory.CreateCommandBuilder();
        connection.Open();
        

        SqlCommand command = new SqlCommand();
        command.CommandText = "SELECT "
          + commandBuilder.QuoteIdentifier("name")
          + " FROM shop WHERE "
          + commandBuilder.QuoteIdentifier("id")
          + " = 1"
          ;
        command.Connection = connection;

        SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          //Console.WriteLine(reader.GetString(0));
        }
      }
    }

    [TestMethod]
    public void WhereAnd()
    {
      Sdx.Db.Where where = new Sdx.Db.Where();

      where.add("id", "1");

      Assert.Equal("[id] = '1'", Sdx.Db.Util.SqlCommandToSql(where.build()));

      where.add("type", 2);
      Assert.Equal("[id] = '1' AND [type] = '2'", Sdx.Db.Util.SqlCommandToSql(where.build()));
    }
  }
}
#endif
