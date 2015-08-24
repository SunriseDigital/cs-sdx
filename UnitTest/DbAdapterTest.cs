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

namespace UnitTest
{
  [TestClass]
  public class DbAdapterTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestQueryLog()
    {
      var db = this.CreateTestDbList()[0].Adapter;

      db.Profiler = new Sdx.Db.Query.Profiler();

      var command = db.CreateCommand();
      command.CommandText = "SELECT * FROM shop WHERE id > @id";

      var param = command.CreateParameter();
      param.ParameterName = "@id";
      param.Value = 1;
      command.Parameters.Add(param);

      using (var con = db.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = db.ExecuteReader(command);
      }

      Assert.Equal(1, db.Profiler.Queries.Count);
      Assert.True(db.Profiler.Queries[0].ElapsedTime > 0);
      Assert.True(db.Profiler.Queries[0].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id", db.Profiler.Queries[0].CommandText);
      Assert.Equal("@id : 1", db.Profiler.Queries[0].FormatedParameters);


      command.CommandText = "SELECT * FROM shop WHERE id > @id AND name = @name";
      param = command.CreateParameter();
      param.ParameterName = "@name";
      param.Value = "foobar";
      command.Parameters.Add(param);

      using (var con = db.CreateConnection())
      {
        con.Open();
        command.Connection = con;
        var reader = db.ExecuteReader(command);
      }

      Assert.Equal(2, db.Profiler.Queries.Count);
      Assert.True(db.Profiler.Queries[1].ElapsedTime > 0);
      Assert.True(db.Profiler.Queries[1].FormatedElapsedTime is String);
      Assert.Equal("SELECT * FROM shop WHERE id > @id AND name = @name", db.Profiler.Queries[1].CommandText);
      Assert.Equal("  @id : 1"+System.Environment.NewLine+"@name : foobar", db.Profiler.Queries[1].FormatedParameters);
    }
  }
}
