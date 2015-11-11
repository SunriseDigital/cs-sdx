using Xunit;
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

      validator = new Sdx.Validation.NotEmpty();
      validator.Messages[Sdx.Validation.NotEmpty.ErrorIsEmpty] = "SWAP MESSAGE DIC";
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
      Assert.False(validator.Inclusive);
      Assert.True(validator.IsValid("9"));
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10未満の数字を入力してください。", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10);
      validator.Inclusive = true;
      Assert.True(validator.IsValid("10"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("10以下の数字を入力してください。", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10, true);
      Assert.True(validator.Inclusive);

      validator = new Sdx.Validation.LessThan(10);
      Assert.False(validator.IsValid("aaa"));
      Assert.Equal("数字を入力してください。", validator.Errors[0].Message);


      validator = new Sdx.Validation.LessThan(10, "%max% and %max%");
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10, true, "%max% and %max%");
      Assert.True(validator.IsValid("10"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);

      long max = Int32.MaxValue;
      ++max;
      validator = new Sdx.Validation.LessThan(max);
      Assert.False(validator.IsValid((max + 1).ToString()));
    }


    [Fact]
    public void TestGreaterThan()
    {
      Sdx.Context.Current.Lang = "ja";

      var validator = new Sdx.Validation.GreaterThan(10);
      Assert.Equal(10, validator.Min);
      Assert.False(validator.Inclusive);
      Assert.True(validator.IsValid("11"));
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10より大きな数字を入力してください。", validator.Errors[0].Message);

      validator = new Sdx.Validation.GreaterThan(10, true);
      Assert.Equal(10, validator.Min);
      Assert.True(validator.IsValid("10"));
      Assert.False(validator.IsValid("9"));
      Assert.Equal("10以上の数字を入力してください。", validator.Errors[0].Message);
      validator.Errors.Clear();
      Assert.False(validator.IsValid("９"));//全角はだめ
      Assert.Equal("数字を入力してください。", validator.Errors[0].Message);


      validator = new Sdx.Validation.GreaterThan(10, true, "%min% and %min%");
      Assert.False(validator.IsValid("9"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);
    }

    [Fact]
    public void TestStringLength()
    {
      Sdx.Context.Current.Lang = "ja";

      var validator = new Sdx.Validation.StringLength(3);
      Assert.Equal(3, validator.Min);
      Assert.Null(validator.Max);
      Assert.True(validator.IsValid("aaa"));
      Assert.False(validator.IsValid("aa"));
      Assert.Equal("3文字以上入力してください（現在2文字）。", validator.Errors[0].Message);

      validator = new Sdx.Validation.StringLength(max: 9);
      Assert.Null(validator.Min);
      Assert.Equal(9, validator.Max);
      Assert.True(validator.IsValid("123456789"));
      Assert.False(validator.IsValid("12345678910"));
      Assert.Equal("9文字までしか入力できません（現在11文字）。", validator.Errors[0].Message);

      validator = new Sdx.Validation.StringLength(4, 6);
      Assert.Equal(4, validator.Min);
      Assert.Equal(6, validator.Max);

      //\r\nが2文字になる件ですが、DBでも2にカウントされるので通してしまうとエラーになってしまう可能性がある。
      //本来フィルターして統一すべき案件なので、Vaildatorでは対応しない方針にします。
      validator = new Sdx.Validation.StringLength(max: 4);
      Assert.False(validator.IsValid("日本語\r\n"));
    }

    [Fact]
    public void TestNumeric()
    {
      Sdx.Context.Current.Lang = "ja";

      var validator = new Sdx.Validation.Numeric();
      Assert.True(validator.IsValid("1234567"));
      Assert.False(validator.IsValid("１２３"));//全角はだめ
      Assert.Equal("数字を入力してください。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestClassMessages()
    {
      Sdx.Context.Current.Lang = "ja";
      var validator = new Test.Validation.Numeric();
      validator.IsValid("aaa");
      Assert.Equal("あへあへうひは", validator.Errors[0].Message);

      Sdx.Context.Current.Lang = "en";
      validator = new Test.Validation.Numeric();
      validator.IsValid("aaa");
      Assert.Equal("Ahe ahe uhiha", validator.Errors[0].Message);
    }

    [Fact]
    public void TestEmail()
    {
      Sdx.Context.Current.Lang = "ja";
      var validator = new Sdx.Validation.Email();

      Assert.True(validator.IsValid("foo@bar.com"));
      Assert.True(validator.IsValid("foo@bar.co.jp"));
      Assert.True(validator.IsValid("foo+bar@bar.hoge"));
      Assert.True(validator.IsValid("foo@hoge.hoge.bar.hoge"));

      //本来許されないけどガラケーで結構見かける
      Assert.True(validator.IsValid("foo?@bar.hoge"));
      Assert.True(validator.IsValid("foo...aa@bar.hoge"));
      Assert.True(validator.IsValid("+foo@foo.bar"));
      Assert.True(validator.IsValid("foo.@foo.bar"));

      Assert.False(validator.IsValid("foo"));
      Assert.False(validator.IsValid("foo@"));
      Assert.False(validator.IsValid("foo@bar"));
      Assert.False(validator.IsValid("foo@bar."));
      Assert.False(validator.IsValid("foo@foo@bar"));

      //http://suzuki.tdiary.net/20130124.html
      //http://it.srad.jp/story/14/08/06/0852210/
      //日本語のメールアドレスは使用可能になったが、普及率と入力ミスによる機会損失をを天秤にかけ今のところ通らないようにしておきます。
      Assert.False(validator.IsValid("日本語@hoge.hoge.bar.hoge"));
      Assert.False(validator.IsValid("aaa@ドメインも日本語.bar.hoge"));

      Assert.Equal("メールアドレスの書式が正しくありません。", validator.Errors[0].Message);

    }

    [Fact]
    public void TestRegex()
    {
      Sdx.Context.Current.Lang = "ja";
      var validator = new Sdx.Validation.Regex("[0-9]+");
      Assert.Equal("[0-9]+", validator.Pattern.ToString());
      Assert.True(validator.IsValid("0123"));
      Assert.False(validator.IsValid("aaaa"));
      Assert.Equal("書式が正しくありません。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestWhitelist()
    {
      Sdx.Context.Current.Lang = "ja";
      var validator = new Sdx.Validation.Whitelist(new string[] {"10", "20"});

      Assert.True(validator.IsValid("10"));
      Assert.True(validator.IsValid("20"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("不正な値です。", validator.Errors[0].Message);
    }
  }
}
