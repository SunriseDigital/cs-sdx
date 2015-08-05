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
      Sdx.Db.Query.Select select;
      
      //Inner
      select = db.Adapter.CreateSelect();
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

      //Left
      select = db.Adapter.CreateSelect();
      select
         .From(new Test.Orm.Table.Shop())
         .LeftJoin(new Test.Orm.Table.Category());

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
        LEFT JOIN {0}category{1} ON {0}shop{1}.category_id = {0}category{1}.id"), db.Command.CommandText);

      select = db.Adapter.CreateSelect();
      select.From(new Test.Orm.Table.Shop());
      select.Context("shop")
        .InnerJoin(
          new Test.Orm.Table.Category(),
          db.Adapter.CreateCondition("{0}.category_id = {1}.id")
            .AddRight("id", "3")
        );

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
        INNER JOIN {0}category{1}
          ON {0}shop{1}.category_id = {0}category{1}.id
            AND {0}category{1}.{0}id{1} = @0"), db.Command.CommandText);

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("3", db.Command.Parameters["@0"].Value);
    }

    [Fact]
    public void TestTableChangeColumn()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunTableChangeColumn(db);
        ExecSql(db);
      }
    }

    private void RunTableChangeColumn(TestDb db)
    {
      Sdx.Db.Query.Select select;

      //simple set
      select = db.Adapter.CreateSelect();
      select.From(new Test.Orm.Table.Shop()).Table.SetColumns("id");
      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1}
        FROM {0}shop{1}"), db.Command.CommandText);

      //add
//      select.Context("shop").Table.AddColumns("name");
//      db.Command = select.Build();
//      Assert.Equal(db.Sql(@"SELECT
//        {0}shop{1}.{0}id{1} AS {0}id@shop{1},
//        {0}shop{1}.{0}name{1} AS {0}name@shop{1}
//        FROM {0}shop{1}"), db.Command.CommandText);
    }
  }
}
