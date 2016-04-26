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
using System.Diagnostics;
using Sdx;
using System.Collections;
using Sdx.Scaffold.Config;
using System.Collections.Generic;
using Sdx.Data;

namespace UnitTest
{
  [TestClass]
  public class TreeJsonTest : BaseTest
  {
    [Fact]
    public void TestTreeJson()
    {
      JsonTest data = new JsonTest();
      data.FooString = "bar";

      List<string> jsonList = new List<string>();
      jsonList.Add("aaa");
      jsonList.Add("bbb");
      jsonList.Add("ccc");
      data.FooList = jsonList;

      Dictionary<string, string> dic = new Dictionary<string, string>();
      dic.Add("name", "hoge");
      dic.Add("ddd", "eee");
      data.FooDic = dic;

      Sdx.Data.Json json = new Sdx.Data.Json(Sdx.Util.Json.Encoder(data));

      Assert.Equal(json.ToValue("Foo"), "bar");
      List<string> list = json.ToList("FooList");
      Assert.Equal(list[0], "aaa");
    }

  }
  public class JsonTest
  {
    public string FooString { get; set; }
    public List<string> FooList { get; set; }
    public Dictionary<string, string> FooDic { get; set; }
  }
}