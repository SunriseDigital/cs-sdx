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
  public class Collection_NameValueCollection : BaseTest
  {
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
    public void TestType()
    {
      var param = new Sdx.Collection.NameValueCollection();
      param.Add("int", "1");
      param.Add("datetime", "2017-03-01");

      Assert.True(param.IsInt32("int"));
      Assert.Equal(1, param.GetInt32Value("int"));
      Assert.Equal(new int[]{1}, param.GetInt32Values("int"));

      Assert.True(param.IsDateTime("datetime"));
      Assert.Equal("2017-03-01", param.GetDateTimeValue("datetime").ToString("yyyy-MM-dd"));
      Assert.Equal(new string[] { "2017-03-01" }, param.GetDateTimeValues("datetime").Select(val => val.ToString("yyyy-MM-dd")));
    }
  }
}
