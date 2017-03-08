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
using System.Threading;
using System.Globalization;
using System.Drawing;
using System.IO;

namespace UnitTest
{
  [TestClass]
  public class Validation_ImageValidator : BaseTest
  {
    [ClassInitialize]
    public static void InitilizeClass(TestContext context)
    {
      Console.WriteLine("FixtureSetUp");
      //最初のテストメソッドを実行する前に一回だけ実行したい処理はここ
    }

    [ClassCleanup]
    public static void CleanupClass()
    {
      Console.WriteLine("FixtureTearDown");
      //全てのテストメソッドが実行された後一回だけ実行する処理はここ
    }

    override protected void SetUp()
    {
      Console.WriteLine("SetUp");
      //各テストメソッドの前に実行する処理はここ
    }

    override protected void TearDown()
    {
      Console.WriteLine("TearDown");
      //各テストメソッドの後に実行する処理はここ
    }

    /// <summary>
    /// このメソッドは消してはダメ
    /// </summary>
    override public void FixtureSetUp()
    {
      _TestTemplate.InitilizeClass(null);
      //ここのクラス名は適宜書き換えてください。
      //MSTestのFixtureSetUpがstaticじゃないとだめだったのでこのような構造になってます。
    }

    /// <summary>
    /// このメソッドは消してはダメ
    /// </summary>
    override public void FixtureTearDown()
    {
      _TestTemplate.CleanupClass();
      //@see FixtureSetUp
    }


    [Fact]
    public void TestImage()
    {
      Console.WriteLine("TestMethod1");
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      //JPEG
      var filePath = "C:\\Projects\\cs-sdx\\UnitTest\\test_image\\100x100.jpg";
      using (FileStream stream = File.OpenRead(filePath))
      {
        var sdxImg = new Sdx.Image(stream);

        Assert.Equal("JPEG", sdxImg.GetFileFormat());
        Assert.Equal(100, sdxImg.Width);
        Assert.Equal(100, sdxImg.Height);
        Assert.Equal(2033, sdxImg.Size);
      }

      //PNG
      filePath = "C:\\Projects\\cs-sdx\\UnitTest\\test_image\\80x80.png";
      using (FileStream stream = File.OpenRead(filePath))
      {
        var sdxImg = new Sdx.Image(stream);

        Assert.Equal("PNG", sdxImg.GetFileFormat());
        Assert.Equal(80, sdxImg.Width);
        Assert.Equal(80, sdxImg.Height);
        Assert.Equal(382, sdxImg.Size);
      }


      //GIF
      filePath = "C:\\Projects\\cs-sdx\\UnitTest\\test_image\\acrobat.gif";
      using (FileStream stream = File.OpenRead(filePath))
      {
        var sdxImg = new Sdx.Image(stream);

        Assert.Equal("GIF", sdxImg.GetFileFormat());
        Assert.Equal(100, sdxImg.Width);
        Assert.Equal(100, sdxImg.Height);
        Assert.Equal(46468 , sdxImg.Size);
      }

      filePath = "C:\\Projects\\cs-sdx\\UnitTest\\test_image\\preloader-2-128px-1.gif";
      using (FileStream stream = File.OpenRead(filePath))
      {
        var sdxImg = new Sdx.Image(stream);

        Assert.Equal("GIF", sdxImg.GetFileFormat());
        Assert.Equal(128, sdxImg.Width);
        Assert.Equal(128, sdxImg.Height);
        Assert.Equal(8165, sdxImg.Size);
      }
    }

    [Fact]
    public void TestMaxSize()
    {
      var filePath = "C:\\Projects\\cs-sdx\\UnitTest\\test_image\\100x100.jpg";
      using (FileStream stream = File.OpenRead(filePath))
      {
        var sdxImg = new Sdx.Image(stream);

        var validator = new Sdx.Validation.Image.MaxSize(maxHeight: 10, maxWidht: 10);
        var isValid = validator.IsValid(sdxImg);
        Assert.False(isValid);
        var count = validator.Errors.Count;
        Assert.Equal(2, count);
        Assert.Equal("高さが10より小さい画像を入力してください。", validator.Errors[0].Message);
        Assert.Equal("幅が10より小さい画像を入力してください。", validator.Errors[1].Message);

        validator = new Sdx.Validation.Image.MaxSize(maxHeight: 10, maxWidht: 101);
        isValid = validator.IsValid(sdxImg);
        Assert.False(isValid);
        count = validator.Errors.Count;
        Assert.Equal(1, count);

        validator = new Sdx.Validation.Image.MaxSize(maxHeight: 101, maxWidht: 10);
        isValid = validator.IsValid(sdxImg);
        Assert.False(isValid);
        count = validator.Errors.Count;
        Assert.Equal(1, count);

        validator = new Sdx.Validation.Image.MaxSize(maxHeight: 101, maxWidht: 101);
        isValid = validator.IsValid(sdxImg);
        Assert.True(isValid);
        count = validator.Errors.Count;
        Assert.Equal(0, count);
      }
    }

  }
}
