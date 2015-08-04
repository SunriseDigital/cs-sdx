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
  public class DbTableTest : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestCreateSelect()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunCreateSelect(db);
        ExecSql(db);
      }
    }

    private void RunCreateSelect(TestDb db)
    {
      Sdx.Db.Table.DefaultAdapter = db.Adapter;

      var shop1 = new Test.Orm.Table.Shop();
      var shop2 = new Test.Orm.Table.Shop();

      Assert.Equal(db.Adapter, shop1.Adapter);
      Assert.Equal(shop1.Adapter, shop2.Adapter);

      var tShop = shop1;
      var select = tShop.Select();

      var command = select.Build();
      Assert.Equal(db.Sql(@"SELECT 
        {0}shop{1}.{0}id{1} AS {0}id@shop{1}, 
        {0}shop{1}.{0}name{1} AS {0}name@shop{1}, 
        {0}shop{1}.{0}category_id{1} AS {0}category_id@shop{1}, 
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1}, 
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1} 
      FROM {0}shop{1}"), command.CommandText);

      tShop.Alias = "foo";
      select = tShop.Select();
      command = select.Build();
      Assert.Equal(db.Sql(@"SELECT 
        {0}foo{1}.{0}id{1} AS {0}id@foo{1}, 
        {0}foo{1}.{0}name{1} AS {0}name@foo{1}, 
        {0}foo{1}.{0}category_id{1} AS {0}category_id@foo{1}, 
        {0}foo{1}.{0}main_image_id{1} AS {0}main_image_id@foo{1}, 
        {0}foo{1}.{0}sub_image_id{1} AS {0}sub_image_id@foo{1} 
      FROM {0}shop{1} AS {0}foo{1}"), command.CommandText);
    }

    [Fact]
    public void TestJoinTable()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunJoinTable(db);
        ExecSql(db);
      }
    }

    private void RunJoinTable(TestDb db)
    {
      Sdx.Db.Table.DefaultAdapter = db.Adapter;

      //db.Adapter.SetNamespace(Sdx.Db.Adapter.Namespace.Context, "Test.Context.Context");


      //var tShop = db.Adapter.CreateTable("Shop");
      //var select = tShop.Select();

      //select.Context("shop").InnerJoin(db.Adapter.CreateTable("Category"));

      var tShop = new Test.Orm.Table.Shop();
      var select = tShop.Select();

      Assert.Equal(typeof(Test.Orm.Table.Shop), select.Context("shop").Table.GetType());

    }
  }
}
