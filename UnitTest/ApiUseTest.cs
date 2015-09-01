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
  public class ApiTest : BaseTest
  {
    [Fact]
    public void TestMethod1()
    {
      Console.WriteLine("TestMethod1");

      Sdx.Api.Json api = new Sdx.Api.Json("http://example.com/path/to/api");

      string token = "asdfghjkl";

      //リクエストに必要な条件をセットする
      api
        .Set("client_id", 1)
        .Set("shop_id", 1234)
        .Set("access_token", token)
        ;

      //結果配列を取得する
      var resp = api.GetResponse();
    }

    [Fact]
    public void TestMethod2()
    {
      Console.WriteLine("TestMethod2");
    }
  }
}