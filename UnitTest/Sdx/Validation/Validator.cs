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
using System.Threading;
using System.Globalization;

namespace UnitTest
{
  [TestClass]
  public class Validation_Validator : BaseTest
  {
    [Fact]
    public void TestNotEmpty()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");
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
      var validator = new Sdx.Validation.NotEmpty();
      validator.MessageDetector = (type, valid) => "SWAP MESSAGE STRING";
      Assert.False(validator.IsValid(""));
      Assert.Equal(1, validator.Errors.Count);
      Assert.Equal("SWAP MESSAGE STRING", validator.Errors[0].Message);
    }

    [Fact]
    public void TestLessThanAndMessagePlaceholder()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      var validator = new Sdx.Validation.LessThan(10);
      Assert.Equal(10, validator.Max);
      Assert.False(validator.IsInclusive);
      Assert.True(validator.IsValid("9"));
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10未満の数字を入力してください。", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10);
      validator.IsInclusive = true;
      Assert.True(validator.IsValid("10"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("10以下の数字を入力してください。", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10, true);
      Assert.True(validator.IsInclusive);

      validator = new Sdx.Validation.LessThan(10);
      Assert.False(validator.IsValid("aaa"));
      Assert.Equal("数字を入力してください。", validator.Errors[0].Message);


      validator = new Sdx.Validation.LessThan(10);
      validator.MessageDetector = (type, valid) => String.Format("{0} and {0}", validator.Max);
      Assert.False(validator.IsValid("10"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);

      validator = new Sdx.Validation.LessThan(10, true);
      validator.MessageDetector = (type, valid) => String.Format("{0} and {0}", validator.Max);
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
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      var validator = new Sdx.Validation.GreaterThan(10);
      Assert.Equal(10, validator.Min);
      Assert.False(validator.IsInclusive);
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


      validator = new Sdx.Validation.GreaterThan(10, true);
      validator.MessageDetector = (type, valid) => String.Format("{0} and {0}", validator.Min);
      Assert.False(validator.IsValid("9"));
      Assert.Equal("10 and 10", validator.Errors[0].Message);
    }

    [Fact]
    public void TestStringLength()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

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
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      var validator = new Sdx.Validation.Numeric();
      Assert.True(validator.IsValid("1234567"));
      Assert.False(validator.IsValid("１２３"));//全角はだめ
      Assert.Equal("数字を入力してください。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestEmail()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");
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
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");
      var validator = new Sdx.Validation.Regex("[0-9]+");
      Assert.Equal("[0-9]+", validator.Pattern.ToString());
      Assert.True(validator.IsValid("0123"));
      Assert.False(validator.IsValid("aaaa"));
      Assert.Equal("書式が正しくありません。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestWhitelist()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");
      var validator = new Sdx.Validation.Whitelist(new string[] { "10", "20" });

      Assert.True(validator.IsValid("10"));
      Assert.True(validator.IsValid("20"));
      Assert.False(validator.IsValid("11"));
      Assert.Equal("不正な値です。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestDateSpan()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      var validator = new Sdx.Validation.DateSpan(min: new DateTime(2015, 10, 12), dateFormat: "yyyy年M月d日");
      Assert.Equal("yyyy年M月d日", validator.DateFormat);
      Assert.True(validator.IsValid("2015/10/12"));
      Assert.True(validator.IsValid("2015-10-12"));
      Assert.False(validator.IsValid("123"));
      Assert.Equal("日付を入力してください。", validator.Errors[0].Message);
      validator.Errors.Clear();
      Assert.False(validator.IsValid("2015/10/11"));
      Assert.Equal("2015年10月12日以降の日付を入力してください。", validator.Errors[0].Message);

      var maxDate = new DateTime(2015, 10, 12);
      validator = new Sdx.Validation.DateSpan(max: maxDate);
      Assert.Equal("d", validator.DateFormat);
      Assert.True(validator.IsValid("2015/10/12"));
      Assert.True(validator.IsValid("2015-10-12"));
      Assert.False(validator.IsValid("2015/10/13"));
      Assert.Equal(maxDate.ToString("d") + "以前の日付を入力してください。", validator.Errors[0].Message);

    }

    [Fact]
    public void TestDateTimeSpan()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      var validator = new Sdx.Validation.DateTimeSpan(min: new DateTime(2015, 10, 12, 16, 30, 0), dateFormat: "yyyy年M月d日 H時m分");
      Assert.Equal("yyyy年M月d日 H時m分", validator.DateFormat);
      Assert.True(validator.IsValid("2015/10/12 16:30"));
      Assert.True(validator.IsValid("2015-10-12 16:30"));
      Assert.False(validator.IsValid("123"));
      Assert.Equal("日時を入力してください。", validator.Errors[0].Message);
      validator.Errors.Clear();
      Assert.False(validator.IsValid("2015/10/11 16:29"));
      Assert.Equal("2015年10月12日 16時30分以降の日時を入力してください。", validator.Errors[0].Message);

      var maxDate = new DateTime(2015, 10, 12, 17, 45, 0);
      validator = new Sdx.Validation.DateTimeSpan(max: maxDate);
      Assert.Null(validator.DateFormat);
      Assert.True(validator.IsValid("2015/10/12 17:45"));
      Assert.True(validator.IsValid("2015-10-12 17:45"));
      Assert.False(validator.IsValid("2015/10/12  17:46"));
      Assert.Equal(maxDate.ToString() + "以前の日時を入力してください。", validator.Errors[0].Message);

    }

    [Fact]
    public void TestDateTime()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");
      var validator = new Sdx.Validation.DateTime();
      Assert.True(validator.IsValid("2014/1/1 1:00:00"));
      Assert.True(validator.IsValid("2014/01/01 01:00:00"));
      Assert.True(validator.IsValid("2014/01/01 01:00"));
      Assert.True(validator.IsValid("14/01/01 01:00:00"));
      Assert.True(validator.IsValid("2014-1-1 1:00:00"));
      Assert.True(validator.IsValid("2014-01-01 01:00:00"));
      Assert.True(validator.IsValid("14-01-01 01:00:00"));
      Assert.True(validator.IsValid("12 Feb 2014 01:00"));


      Assert.False(validator.IsValid("123"));
      Assert.False(validator.IsValid("2014-01-01"));
      Assert.Equal("日時を入力してください。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestDate()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");
      var validator = new Sdx.Validation.Date();
      Assert.True(validator.IsValid("2014/1/1"));
      Assert.True(validator.IsValid("2014/01/01"));
      Assert.True(validator.IsValid("14/01/01"));
      Assert.True(validator.IsValid("2014-1-1"));
      Assert.True(validator.IsValid("2014-01-01"));
      Assert.True(validator.IsValid("14-01-01"));
      Assert.True(validator.IsValid("12 Feb 2014"));

      Assert.False(validator.IsValid("123"));
      Assert.False(validator.IsValid("2014-01-01 00:00:00"));
      Assert.Equal("日付を入力してください。", validator.Errors[0].Message);
    }

    [Fact]
    public void TestStringLengthEnglish()
    {
      Sdx.Context.Current.Culture = new CultureInfo("en");

      var validator = new Sdx.Validation.StringLength(3);
      Assert.Equal(3, validator.Min);
      Assert.Null(validator.Max);
      Assert.True(validator.IsValid("aaa"));
      Assert.False(validator.IsValid("a"));
      Assert.Equal("Please enter 3 or more characters, 1 character now.", validator.Errors[0].Message);

      validator = new Sdx.Validation.StringLength(3);
      Assert.False(validator.IsValid("aa"));
      Assert.Equal("Please enter 3 or more characters, 2 characters now.", validator.Errors[0].Message);

      validator = new Sdx.Validation.StringLength(max: 1);
      Assert.True(validator.IsValid("1"));
      Assert.False(validator.IsValid("12"));
      Assert.Equal("You can enter only 1 character, 2 characters now.", validator.Errors[0].Message);

      validator = new Sdx.Validation.StringLength(max: 9);
      Assert.True(validator.IsValid("123456789"));
      Assert.False(validator.IsValid("12345678910"));
      Assert.Equal("You can enter only 9 characters, 11 characters now.", validator.Errors[0].Message);
    }
  }
}
