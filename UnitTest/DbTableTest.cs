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
      var tShop = new Test.Orm.Table.Shop();
      var select = db.Adapter.CreateSelect();

      select.From(tShop);

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT 
        {0}shop{1}.{0}id{1} AS {0}id@shop{1}, 
        {0}shop{1}.{0}name{1} AS {0}name@shop{1}, 
        {0}shop{1}.{0}category_id{1} AS {0}category_id@shop{1}, 
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1}, 
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1} 
      FROM {0}shop{1}"), db.Command.CommandText);

      select = db.Adapter.CreateSelect();
      select.From(tShop, "foo");
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT 
        {0}foo{1}.{0}id{1} AS {0}id@foo{1}, 
        {0}foo{1}.{0}name{1} AS {0}name@foo{1}, 
        {0}foo{1}.{0}category_id{1} AS {0}category_id@foo{1}, 
        {0}foo{1}.{0}main_image_id{1} AS {0}main_image_id@foo{1}, 
        {0}foo{1}.{0}sub_image_id{1} AS {0}sub_image_id@foo{1} 
      FROM {0}shop{1} AS {0}foo{1}"), db.Command.CommandText);
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
      var select = db.Adapter.CreateSelect();
      select.From(new Test.Orm.Table.Shop());

      Assert.Equal(typeof(Test.Orm.Table.Shop), select.Context("shop").Table.GetType());

      select.Context("shop").InnerJoin(new Test.Orm.Table.Category());

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
          {0}shop{1}.{0}id{1} AS {0}id@shop{1},
          {0}shop{1}.{0}name{1} AS {0}name@shop{1},
          {0}shop{1}.{0}category_id{1} AS {0}category_id@shop{1},
          {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
          {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
          {0}category{1}.{0}id{1} AS {0}id@category{1},
          {0}category{1}.{0}name{1} AS {0}name@category{1},
          {0}category{1}.{0}code{1} AS {0}code@category{1},
          {0}category{1}.{0}category_type_id{1} AS {0}category_type_id@category{1}
        FROM {0}shop{1} 
        INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id"), db.Command.CommandText);

      select = db.Adapter.CreateSelect();
      select.From(new Test.Orm.Table.Shop());
      select.Context("shop").InnerJoin(new Test.Orm.Table.Category(), "{0}.category_id = {1}.id AND {1}.id = 1");

      //conditionの上書き
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
          {0}shop{1}.{0}id{1} AS {0}id@shop{1},
          {0}shop{1}.{0}name{1} AS {0}name@shop{1},
          {0}shop{1}.{0}category_id{1} AS {0}category_id@shop{1},
          {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
          {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
          {0}category{1}.{0}id{1} AS {0}id@category{1},
          {0}category{1}.{0}name{1} AS {0}name@category{1},
          {0}category{1}.{0}code{1} AS {0}code@category{1},
          {0}category{1}.{0}category_type_id{1} AS {0}category_type_id@category{1}
        FROM {0}shop{1} 
        INNER JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id AND {0}category{1}.id = 1"), db.Command.CommandText);
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
      Sdx.Db.Query.Select select = db.Adapter.CreateSelect();

      //AddRight
      select.From("shop").Columns("*");
      var cond = select.CreateCondition("{0}.category_id = {1}.id").AddRight("id", "1");
      var command = select.Build();
      Assert.Equal(db.Sql("{{0}}.category_id = {{1}}.id AND {{1}}.{0}id{1} = @0"), cond.Build(command.Parameters));

      //AddLeft
      select = db.Adapter.CreateSelect();
      select.From("shop").Columns("*");
      cond = select.CreateCondition("{0}.category_id = {1}.id").AddLeft("id", "1");
      command = select.Build();
      Assert.Equal(db.Sql("{{0}}.category_id = {{1}}.id AND {{0}}.{0}id{1} = @0"), cond.Build(command.Parameters));

      //InnerJoin
      //select = db.Adapter.CreateSelect();
      //select.From("shop").Columns("*");
      //select.Context("shop").InnerJoin(
      //  "category",
      //  select.CreateCondition(),
      //).Columns("*");
    }
  }
}
