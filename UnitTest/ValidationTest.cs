﻿using Xunit;
using UnitTest.DummyClasses;
using System.Collections.Generic;

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
  public class ValidationTest : BaseTest
  {
    [Fact]
    public void TestNotEmpty()
    {
      Sdx.Context.Current.Lang = "ja";
      var validator = new Sdx.Validation.NotEmpty();

      Assert.True(validator.IsValid("aaa"));
      Assert.Equal(0, validator.Errors.Count);

      validator.Errors.Clear();
      Assert.True(validator.IsValid(new string[] { "aaa" }));
      Assert.Equal(0, validator.Errors.Count);

      validator.Errors.Clear();
      Assert.True(validator.IsValid(new string[] { "" }));
      Assert.Equal(0, validator.Errors.Count);

      validator.Errors.Clear();
      Assert.False(validator.IsValid(""));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("必須項目です。", validator.Errors[0].Message);

      validator.Errors.Clear();
      Assert.False(validator.IsValid(new string[] { }));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("必須項目です。", validator.Errors[0].Message);

      string nullString = null;
      validator.Errors.Clear();
      Assert.False(validator.IsValid(nullString));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("必須項目です。", validator.Errors[0].Message);

      string[] nullArray = null;
      validator.Errors.Clear();
      Assert.False(validator.IsValid(nullArray));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("必須項目です。", validator.Errors[0].Message);
    }


    [Fact]
    public void TestSwapMessage()
    {
      var validator = new Sdx.Validation.NotEmpty("SWAP MESSAGE STRING");
      Assert.False(validator.IsValid(""));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("SWAP MESSAGE STRING", validator.Errors[0].Message);

      validator = new Sdx.Validation.NotEmpty(new Dictionary<string, string> {
        { Sdx.Validation.NotEmpty.ErrorIsEmpty, "SWAP MESSAGE DIC"}
      });
      Assert.False(validator.IsValid(""));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("SWAP MESSAGE DIC", validator.Errors[0].Message);

    }

    [Fact]
    public void TestLessThanAndMessagePlaceholder()
    {
      Sdx.Context.Current.Lang = "ja";

      var validator = new Sdx.Validation.LessThan(10);

      Assert.Equal(10, validator.Max);
      Assert.True(validator.IsValid("9"));
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10未満の数字を入力してください。", validator.Errors[0].Message);

      validator.Errors.Clear();
      validator.Inclusive = true;
      Assert.True(validator.IsValid("10"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("10以下の数字を入力してください。", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10, true);
      Assert.True(validator.Inclusive);


      validator = new Sdx.Validation.LessThan(10, "<SDX>max</SDX> and <SDX>max</SDX>");
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10, true, "<SDX>max</SDX> and <SDX>max</SDX>");
      Assert.True(validator.IsValid("10"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);
    }
  }
}
