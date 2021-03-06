﻿using Xunit;
using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;
using System.Collections.Generic;

namespace UnitTest
{
  [TestClass]
  public class Db_Table : BaseDbTest
  {
    [ClassInitialize]
    public new static void InitilizeClass(TestContext context)
    {
      BaseDbTest.InitilizeClass(context);
    }

    [Fact]
    public void TestFromTable()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFromTable(db);
        ExecSql(db);
      }
    }

    private void RunFromTable(TestDb db)
    {
      var tShop = new Test.Orm.Table.Shop();
      var select = db.Adapter.CreateSelect();

      select.AddFrom(tShop);


      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT 
        {0}shop{1}.{0}id{1} AS {0}id@shop{1}, 
        {0}shop{1}.{0}name{1} AS {0}name@shop{1}, 
        {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1}, 
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1}, 
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
        {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
        {0}shop{1}.{0}password{1} AS {0}password@shop{1},
        {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1} 
      FROM {0}shop{1}"), db.Command.CommandText);

      select = db.Adapter.CreateSelect();
      select.AddFrom(tShop, "foo");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT 
        {0}foo{1}.{0}id{1} AS {0}id@foo{1}, 
        {0}foo{1}.{0}name{1} AS {0}name@foo{1}, 
        {0}foo{1}.{0}area_id{1} AS {0}area_id@foo{1}, 
        {0}foo{1}.{0}main_image_id{1} AS {0}main_image_id@foo{1}, 
        {0}foo{1}.{0}sub_image_id{1} AS {0}sub_image_id@foo{1},
        {0}foo{1}.{0}login_id{1} AS {0}login_id@foo{1},
        {0}foo{1}.{0}password{1} AS {0}password@foo{1},
        {0}foo{1}.{0}created_at{1} AS {0}created_at@foo{1} 
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
      Sdx.Db.Sql.Select select;

      //Inner
      select = db.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Shop());

      Assert.Equal(typeof(Test.Orm.Table.Shop), select.Context("shop").Table.GetType());

      select.Context("shop").InnerJoin(new Test.Orm.Table.Area());


      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
          {0}shop{1}.{0}id{1} AS {0}id@shop{1},
          {0}shop{1}.{0}name{1} AS {0}name@shop{1},
          {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
          {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
          {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
          {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
          {0}shop{1}.{0}password{1} AS {0}password@shop{1},
          {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
          {0}area{1}.{0}id{1} AS {0}id@area{1},
          {0}area{1}.{0}name{1} AS {0}name@area{1},
          {0}area{1}.{0}code{1} AS {0}code@area{1},
          {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
          {0}area{1}.{0}sequence{1} AS {0}sequence@area{1}
        FROM {0}shop{1} 
        INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}"), db.Command.CommandText);

      //Left
      select = db.Adapter.CreateSelect();
      select
         .AddFrom(new Test.Orm.Table.Shop())
         .LeftJoin(new Test.Orm.Table.Area());


      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
          {0}shop{1}.{0}id{1} AS {0}id@shop{1},
          {0}shop{1}.{0}name{1} AS {0}name@shop{1},
          {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
          {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
          {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
          {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
          {0}shop{1}.{0}password{1} AS {0}password@shop{1},
          {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
          {0}area{1}.{0}id{1} AS {0}id@area{1},
          {0}area{1}.{0}name{1} AS {0}name@area{1},
          {0}area{1}.{0}code{1} AS {0}code@area{1},
          {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
          {0}area{1}.{0}sequence{1} AS {0}sequence@area{1}
        FROM {0}shop{1} 
        LEFT JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}"), db.Command.CommandText);

      select = db.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Shop());
      select.Context("shop")
        .InnerJoin(
          new Test.Orm.Table.Area(),
          db.Adapter.CreateCondition()
            .Add(
              new Sdx.Db.Sql.Column("area_id", "shop"),
              new Sdx.Db.Sql.Column("id", "area")
            ).Add(new Sdx.Db.Sql.Column("id", "area"), "3")
        );

      //conditionの上書き

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
          {0}shop{1}.{0}id{1} AS {0}id@shop{1},
          {0}shop{1}.{0}name{1} AS {0}name@shop{1},
          {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
          {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
          {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
          {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
          {0}shop{1}.{0}password{1} AS {0}password@shop{1},
          {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
          {0}area{1}.{0}id{1} AS {0}id@area{1},
          {0}area{1}.{0}name{1} AS {0}name@area{1},
          {0}area{1}.{0}code{1} AS {0}code@area{1},
          {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
          {0}area{1}.{0}sequence{1} AS {0}sequence@area{1}
        FROM {0}shop{1} 
        INNER JOIN {0}area{1}
          ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
            AND {0}area{1}.{0}id{1} = @0"), db.Command.CommandText);

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
      Sdx.Db.Sql.Select select;

      //simple set
      select = db.Adapter.CreateSelect();
      select.AddFrom(new Test.Orm.Table.Shop()).Table.SetColumns("id");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1}
        FROM {0}shop{1}"), db.Command.CommandText);

      //add
      select.Context("shop").Table.AddColumns("name");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1},
        {0}shop{1}.{0}name{1} AS {0}name@shop{1}
        FROM {0}shop{1}"), db.Command.CommandText);
    }

    [Fact]
    public void TestTableComplexJoin()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunTableComplexJoin(db);
        ExecSql(db);
      }
    }

    private void RunTableComplexJoin(TestDb db)
    {
      Sdx.Db.Sql.Select select;

      select = db.Adapter.CreateSelect();

      select
          .AddFrom(new Test.Orm.Table.Shop())
          .InnerJoin(new Test.Orm.Table.Area())
          .InnerJoin(new Test.Orm.Table.LargeArea());

      select.Context("shop").InnerJoin(new Test.Orm.Table.Image(), "main_image");
      select.Context("shop").LeftJoin(new Test.Orm.Table.Image(), "sub_image");


      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1},
        {0}shop{1}.{0}name{1} AS {0}name@shop{1},
        {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
        {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
        {0}shop{1}.{0}password{1} AS {0}password@shop{1},
        {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
        {0}area{1}.{0}id{1} AS {0}id@area{1},
        {0}area{1}.{0}name{1} AS {0}name@area{1},
        {0}area{1}.{0}code{1} AS {0}code@area{1},
        {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
        {0}area{1}.{0}sequence{1} AS {0}sequence@area{1},
        {0}large_area{1}.{0}id{1} AS {0}id@large_area{1},
        {0}large_area{1}.{0}name{1} AS {0}name@large_area{1},
        {0}large_area{1}.{0}code{1} AS {0}code@large_area{1},
        {0}main_image{1}.{0}id{1} AS {0}id@main_image{1},
        {0}main_image{1}.{0}path{1} AS {0}path@main_image{1},
        {0}sub_image{1}.{0}id{1} AS {0}id@sub_image{1},
        {0}sub_image{1}.{0}path{1} AS {0}path@sub_image{1}
        FROM {0}shop{1}
        INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
        INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
        INNER JOIN {0}image{1} AS {0}main_image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}main_image{1}.{0}id{1}
        LEFT JOIN {0}image{1} AS {0}sub_image{1} ON {0}shop{1}.{0}sub_image_id{1} = {0}sub_image{1}.{0}id{1}"), db.Command.CommandText);

      select.Context("shop").Where.Add("id", "1");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1},
        {0}shop{1}.{0}name{1} AS {0}name@shop{1},
        {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
        {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
        {0}shop{1}.{0}password{1} AS {0}password@shop{1},
        {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
        {0}area{1}.{0}id{1} AS {0}id@area{1},
        {0}area{1}.{0}name{1} AS {0}name@area{1},
        {0}area{1}.{0}code{1} AS {0}code@area{1},
        {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
        {0}area{1}.{0}sequence{1} AS {0}sequence@area{1},
        {0}large_area{1}.{0}id{1} AS {0}id@large_area{1},
        {0}large_area{1}.{0}name{1} AS {0}name@large_area{1},
        {0}large_area{1}.{0}code{1} AS {0}code@large_area{1},
        {0}main_image{1}.{0}id{1} AS {0}id@main_image{1},
        {0}main_image{1}.{0}path{1} AS {0}path@main_image{1},
        {0}sub_image{1}.{0}id{1} AS {0}id@sub_image{1},
        {0}sub_image{1}.{0}path{1} AS {0}path@sub_image{1}
        FROM {0}shop{1}
        INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
        INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
        INNER JOIN {0}image{1} AS {0}main_image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}main_image{1}.{0}id{1}
        LEFT JOIN {0}image{1} AS {0}sub_image{1} ON {0}shop{1}.{0}sub_image_id{1} = {0}sub_image{1}.{0}id{1}
        WHERE {0}shop{1}.{0}id{1} = @0"), db.Command.CommandText);

      Assert.Equal(1, db.Command.Parameters.Count);
      Assert.Equal("1", db.Command.Parameters["@0"].Value);

      select.Context("area").Where.Add("code", "foo");

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1},
        {0}shop{1}.{0}name{1} AS {0}name@shop{1},
        {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
        {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
        {0}shop{1}.{0}password{1} AS {0}password@shop{1},
        {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
        {0}area{1}.{0}id{1} AS {0}id@area{1},
        {0}area{1}.{0}name{1} AS {0}name@area{1},
        {0}area{1}.{0}code{1} AS {0}code@area{1},
        {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
        {0}area{1}.{0}sequence{1} AS {0}sequence@area{1},
        {0}large_area{1}.{0}id{1} AS {0}id@large_area{1},
        {0}large_area{1}.{0}name{1} AS {0}name@large_area{1},
        {0}large_area{1}.{0}code{1} AS {0}code@large_area{1},
        {0}main_image{1}.{0}id{1} AS {0}id@main_image{1},
        {0}main_image{1}.{0}path{1} AS {0}path@main_image{1},
        {0}sub_image{1}.{0}id{1} AS {0}id@sub_image{1},
        {0}sub_image{1}.{0}path{1} AS {0}path@sub_image{1}
        FROM {0}shop{1}
        INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
        INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
        INNER JOIN {0}image{1} AS {0}main_image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}main_image{1}.{0}id{1}
        LEFT JOIN {0}image{1} AS {0}sub_image{1} ON {0}shop{1}.{0}sub_image_id{1} = {0}sub_image{1}.{0}id{1}
        WHERE {0}shop{1}.{0}id{1} = @0
        AND {0}area{1}.{0}code{1} = @1"), db.Command.CommandText);
      Assert.Equal(2, db.Command.Parameters.Count);
      Assert.Equal("foo", db.Command.Parameters["@1"].Value);

      select.Context("sub_image").Where.Add(
        db.Adapter.CreateCondition()
         .Add("id", "10")
         .AddIsNullOr("id")
      );

      db.Command = select.Build();
      Assert.Equal(db.Sql(@"SELECT
        {0}shop{1}.{0}id{1} AS {0}id@shop{1},
        {0}shop{1}.{0}name{1} AS {0}name@shop{1},
        {0}shop{1}.{0}area_id{1} AS {0}area_id@shop{1},
        {0}shop{1}.{0}main_image_id{1} AS {0}main_image_id@shop{1},
        {0}shop{1}.{0}sub_image_id{1} AS {0}sub_image_id@shop{1},
        {0}shop{1}.{0}login_id{1} AS {0}login_id@shop{1},
        {0}shop{1}.{0}password{1} AS {0}password@shop{1},
        {0}shop{1}.{0}created_at{1} AS {0}created_at@shop{1},
        {0}area{1}.{0}id{1} AS {0}id@area{1},
        {0}area{1}.{0}name{1} AS {0}name@area{1},
        {0}area{1}.{0}code{1} AS {0}code@area{1},
        {0}area{1}.{0}large_area_id{1} AS {0}large_area_id@area{1},
        {0}area{1}.{0}sequence{1} AS {0}sequence@area{1},
        {0}large_area{1}.{0}id{1} AS {0}id@large_area{1},
        {0}large_area{1}.{0}name{1} AS {0}name@large_area{1},
        {0}large_area{1}.{0}code{1} AS {0}code@large_area{1},
        {0}main_image{1}.{0}id{1} AS {0}id@main_image{1},
        {0}main_image{1}.{0}path{1} AS {0}path@main_image{1},
        {0}sub_image{1}.{0}id{1} AS {0}id@sub_image{1},
        {0}sub_image{1}.{0}path{1} AS {0}path@sub_image{1}
        FROM {0}shop{1}
        INNER JOIN {0}area{1} ON {0}shop{1}.{0}area_id{1} = {0}area{1}.{0}id{1}
        INNER JOIN {0}large_area{1} ON {0}area{1}.{0}large_area_id{1} = {0}large_area{1}.{0}id{1}
        INNER JOIN {0}image{1} AS {0}main_image{1} ON {0}shop{1}.{0}main_image_id{1} = {0}main_image{1}.{0}id{1}
        LEFT JOIN {0}image{1} AS {0}sub_image{1} ON {0}shop{1}.{0}sub_image_id{1} = {0}sub_image{1}.{0}id{1}
        WHERE {0}shop{1}.{0}id{1} = @0
        AND {0}area{1}.{0}code{1} = @1
        AND ({0}sub_image{1}.{0}id{1} = @2 OR {0}sub_image{1}.{0}id{1} IS NULL)"), db.Command.CommandText);
      Assert.Equal(3, db.Command.Parameters.Count);
      Assert.Equal("10", db.Command.Parameters["@2"].Value);
    }

    [Fact]
    public void TestAutoValidate()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunAutoValidate(db);
        ExecSql(db);
      }
    }

    private void RunAutoValidate(TestDb testDb)
    {
      var column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.Integer,
        isAutoIncrement: true,
        isNotNull: true,
        maxLength: 32
      );

      var record = new Test.Orm.Shop();

      var list = column.CreateValidatorList(record);
      Assert.Equal(3, list.Count);
      Assert.IsType<Sdx.Validation.Numeric>(list[0]);
      Assert.IsType<Sdx.Validation.GreaterThan>(list[1]);
      Assert.Equal(Int32.MinValue, ((Sdx.Validation.GreaterThan)list[1]).Min);
      Assert.IsType<Sdx.Validation.LessThan>(list[2]);
      Assert.Equal(Int32.MaxValue, ((Sdx.Validation.LessThan)list[2]).Max);

      column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.UnsignedInteger,
        isNotNull: false,
        maxLength: 32
      );

      list = column.CreateValidatorList(record);
      Assert.Equal(3, list.Count);
      Assert.IsType<Sdx.Validation.Numeric>(list[0]);
      Assert.IsType<Sdx.Validation.GreaterThan>(list[1]);
      Assert.Equal(UInt32.MinValue, ((Sdx.Validation.GreaterThan)list[1]).Min);
      Assert.IsType<Sdx.Validation.LessThan>(list[2]);
      Assert.Equal(UInt32.MaxValue, ((Sdx.Validation.LessThan)list[2]).Max);


      column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.Float,
        isNotNull: false,
        maxLength: 32//Float系は長さを無視
      );

      list = column.CreateValidatorList(record);
      Assert.Equal(1, list.Count);
      Assert.IsType<Sdx.Validation.Numeric>(list[0]);


      column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.String,
        isNotNull: false,
        maxLength: 100
      );

      list = column.CreateValidatorList(record);
      Assert.Equal(1, list.Count);
      Assert.IsType<Sdx.Validation.StringLength>(list[0]);
      Assert.Equal(100, ((Sdx.Validation.StringLength)list[0]).Max);


      column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.DateTime,
        isNotNull: false
      );

      list = column.CreateValidatorList(record);
      Assert.Equal(1, list.Count);
      Assert.IsType<Sdx.Validation.DateTime>(list[0]);

      column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.Date,
        isNotNull: false
      );

      list = column.CreateValidatorList(record);
      Assert.Equal(1, list.Count);
      Assert.IsType<Sdx.Validation.Date>(list[0]);

      //数字系でMaxLengthが空の時例外になっていたのでついか。
      column = new Sdx.Db.Table.Column(
        "id",
        type: Sdx.Db.Table.ColumnType.Integer,
        isNotNull: false
      );

      list = column.CreateValidatorList(record);
      Assert.Equal(1, list.Count);
      Assert.IsType<Sdx.Validation.Numeric>(list[0]);

      //isAutoFill


      var db = testDb.Adapter;
      using(var conn = db.CreateConnection())
      {
        conn.Open();
        column = new Sdx.Db.Table.Column(
          Sdx.Db.Record.AutoCreateDateColumn,
          type: Sdx.Db.Table.ColumnType.DateTime,
          isNotNull: true
        );

        list = column.CreateValidatorList(record);
        Assert.Equal(1, list.Count);
        Assert.IsType<Sdx.Validation.DateTime>(list[0]);

        record = (Test.Orm.Shop)(new Test.Orm.Table.Shop()).FetchRecordByPkey(conn, "1");
        list = column.CreateValidatorList(record);
        Assert.Equal(2, list.Count);
        Assert.IsType<Sdx.Validation.NotEmpty>(list[0]);
      }
      
    }

    [Fact]
    public void TestFetchRecordByPkey()
    {
      foreach (TestDb db in this.CreateTestDbList())
      {
        RunFetchRecordByPkey(db);
        ExecSql(db);
      }
    }

    private void RunFetchRecordByPkey(TestDb testDb)
    {
      var db = testDb.Adapter;

      //single pkey
      {
        var tShop = new Test.Orm.Table.Shop();
        using (var conn = db.CreateConnection())
        {
          conn.Open();
          var shop = tShop.FetchRecordByPkey(conn, 1);
          Assert.Equal(1, shop.GetInt32("id"));

          shop = tShop.FetchRecordByPkey(conn, "2");
          Assert.Equal(2, shop.GetInt32("id"));
        }
      }

      //multiple key
      {
        var tShopCategory = new Test.Orm.Table.ShopCategory();
        using (var conn = db.CreateConnection())
        {
          conn.Open();

          var inserts = new List<Sdx.Db.Sql.Insert>();
          inserts.Add(db.CreateInsert());
          inserts[0].Into = "shop_category";
          inserts[0].AddColumnValue("shop_id", 1);
          inserts[0].AddColumnValue("category_id", 2);
          inserts.Add(db.CreateInsert());
          inserts[1].Into = "shop_category";
          inserts[1].AddColumnValue("shop_id", 2);
          inserts[1].AddColumnValue("category_id", 3);
          conn.BeginTransaction();
          inserts.ForEach(ins => conn.Execute(ins));
          conn.Commit();
          

          var shopCategory = tShopCategory.FetchRecordByPkey(conn, new Dictionary<string, object> 
          { 
            {"shop_id", 1},
            {"category_id", 2},
          });
          Assert.Equal(1, shopCategory.GetInt32("shop_id"));
          Assert.Equal(2, shopCategory.GetInt32("category_id"));

          shopCategory = tShopCategory.FetchRecordByPkey(conn, new Dictionary<string, object> 
          { 
            {"shop_id", "2"},
            {"category_id", "3"},
          });
          Assert.Equal(2, shopCategory.GetInt32("shop_id"));
          Assert.Equal(3, shopCategory.GetInt32("category_id"));
        }
      }
    }
  }
}
