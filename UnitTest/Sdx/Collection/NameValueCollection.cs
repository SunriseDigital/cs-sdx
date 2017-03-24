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
using System.Linq;
using System.Collections.Generic;

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
    public void TestDictionary()
    {
      var param = new Sdx.Collection.NameValueCollection();
      param.Add("dic@id", "1");
      param.Add("dic@name", "foobar");
      param.Add("dic@date", "2017-03-01");

      param.Add("dic@id", "2");
      param.Add("dic@name", "hoge");
      param.Add("dic@date", "2017-03-02");

      param.Add("dic@id", "3");
      param.Add("dic@name", "");
      param.Add("dic@date", "2017-03-03");

      var expectedDics = new List<Dictionary<string, string>>{
        new Dictionary<string, string>{
          {"id", "1"},
          {"name", "foobar"},
          {"date", "2017-03-01"}
        },
        new Dictionary<string, string>{
          {"id", "2"},
          {"name", "hoge"},
          {"date", "2017-03-02"}
        },
        new Dictionary<string, string>{
          {"id", "3"},
          {"name", ""},
          {"date", "2017-03-03"}
        }
      };
      Assert.Equal(expectedDics, param.GetDictionaryValues("dic", "@"));
    }
  }
}
