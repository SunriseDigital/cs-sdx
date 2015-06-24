//MSTestはtravis.ciでテストが動かないのでデバッグモード時にのみコンパイルします。
#if DEBUG
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Data.Common;
using System.Data;
using Sdx.Db;

using Assert = Xunit.Assert;
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;

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

    [Fact]
    public void TestMethod1()
    {

      using (SqlConnection connection = CreateConnection())
      {
        DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
        DbCommandBuilder commandBuilder = factory.CreateCommandBuilder();
        connection.Open();
        

        SqlCommand command = new SqlCommand();
        command.CommandText = "SELECT [shop].[name] as name@shop, [category].[name] as name@category FROM [shop]"
          + " INNER JOIN [category] ON [category].[id] = [shop].[category_id]"
          + " WHERE [shop].[id] = @shop@id"
          ;

        command.Parameters.AddWithValue("@shop@id", "1");

        command.Connection = connection;

        SqlDataReader reader = command.ExecuteReader();
        List<Dictionary<string, string>> list = Sdx.Db.Util.CreateDictinaryList(reader);
        Console.WriteLine(Sdx.DebugTool.Debug.Dump(list));
      }
    }

    [Fact]
    public void WhereWithTable()
    {
      Sdx.Db.Where where = new Sdx.Db.Where();
      where.add("id", "1", "shop");

      Assert.Equal("[shop].[id] = '1'", Sdx.Db.Util.SqlCommandToSql(where.build()));

      where.add("type", 2, "category");
      Assert.Equal("[shop].[id] = '1' AND [category].[type] = '2'", Sdx.Db.Util.SqlCommandToSql(where.build()));
    }
  }
}
#endif
