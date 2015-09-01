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

      String ApiName = "api-name";
      Sdx.Api.Json Api = new Sdx.Api.Json(ApiName);

      String Token = "asdfghjkl";

      //リクエストに必要な条件をセットする
      Api
        ->SetClientId(1)
        ->SetShopId(1234)
        ->SetAccessToken(Token)
        ;

      //結果配列を取得する
      var Resp = Api->GetResponse();
    }

    [Fact]
    public void TestMethod2()
    {
      Console.WriteLine("TestMethod2");
    }
  }
}