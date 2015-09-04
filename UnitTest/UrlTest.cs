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
using System.Collections.Generic;

namespace UnitTest
{
  [TestClass]
  public class UrlTest : BaseTest
  {
    [Fact]
    public void TestMethod1()
    {
      Console.WriteLine("TestMethod1");
      //コンストラクタで、URLの分解・解析は済ませておく
      var url = new Sdx.Web.Url("http://example.com/path/to/api?foo=bar&hoge=huga");

      //コンストラクタの解析が意図通りに行われているか、各部品ごとに確認
      Assert.Equal("example.com", url.GetDomain());
      Assert.Equal(Sdx.Web.Url.Protocol.Http, url.GetProtocol());
      Assert.Equal("bar", url.GetParam("foo"));
      Assert.Equal("huga", url.GetParam("hoge"));
      Assert.Equal("/path/to/api", url.GetPath());

      //新しくパラメータを追加した場合、正しくクエリが生成されているか
      url.SetParam("key", "value");
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //Setterではなくコンストラクタの引数にパラメータを渡した場合の挙動
      //オブジェクトが持つデータ自体には変わらないようにする
      var param = new Dictionary<string, string>();
      param["new"] = "newValue";
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value&new=newValue", url.Build(param));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //コンストラクタの引数に配列を渡した場合の挙動
      var array = new String[] { "key" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga", url.Build(array));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());
    }

    [Fact]
    public void TestMethod2()
    {
      Console.WriteLine("TestMethod2");
    }
  }
}
