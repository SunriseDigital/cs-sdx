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
    public void TestMultipleParams()
    {
      Console.WriteLine("TestMethod1");
      //コンストラクタで、URLの分解・解析は済ませておく
      var url = new Sdx.Web.Url("http://example.com/path/to/api?foo=bar&hoge=huga");

      //コンストラクタの解析が意図通りに行われているか、各部品ごとに確認
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("bar", url.Param["foo"]);
      Assert.Equal("huga", url.Param["hoge"]);
      Assert.Equal("/path/to/api", url.LocalPath);

      //新しくパラメータを追加した場合、正しくクエリが生成されているか
      url.Param["key"] = "value";
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //Setterではなくコンストラクタの引数にパラメータを渡した場合の挙動
      //オブジェクトが持つデータ自体は変わらないようにする
      var param = new Dictionary<string, string>();
      param["new"] = "newValue";
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value&new=newValue", url.Build(param));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //同じキー名のパラメータを引数に渡した場合の挙動。上書きされることを期待
      var dic = new Dictionary<string, string>() { { "key", "newValue" } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=newValue", url.Build(dic));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //コンストラクタの引数に配列を渡した場合の挙動。指定したキーがクエリから除かれているようにする。
      //同じくオブジェクトが持つデータ自体が変わらないようにする
      var array = new String[] { "key" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga", url.Build(array));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //引数をListにした際の挙動。基本は array のときと同じ挙動を期待
      var list = new List<string>() { "key" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga", url.Build(list));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());
    }

    [Fact]
    public void TestNonParams()
    {
      Console.WriteLine("TestNonParams");
      //クエリなしURLを渡す
      var url = new Sdx.Web.Url("http://example.com/path/to/api");

      //各部品の取得
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("/path/to/api", url.LocalPath);

      //パラメータの追加(プロパティ経由)
      url.Param["key"] = "value";
      Assert.Equal("value", url.Param["key"]);
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build());

      //パラメータの追加(引数で)
      var param = new Dictionary<string, string>() { {"foo", "bar"} };
      Assert.Equal("http://example.com/path/to/api?key=value&foo=bar", url.Build(param));
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build());

      //パラメータ削除
      var array = new string[] { "key" };
      Assert.Equal("http://example.com/path/to/api", url.Build(array));
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build());

      var list = new List<string>() { "key" };
      Assert.Equal("http://example.com/path/to/api", url.Build(list));
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build());
    }

    [Fact]
    public void TestSingleParam()
    {
      Console.WriteLine("TestSingleParam");
      //パラメータが1つだけ(クエリに'＆'が無い)
      var url = new Sdx.Web.Url("http://example.com/path/to/api?foo=bar");

      //各部品の取得
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("/path/to/api", url.LocalPath);

      //パラメータの追加(プロパティ経由)
      url.Param["key"] = "value";
      Assert.Equal("value", url.Param["key"]);
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build());

      //パラメータの追加(引数で)
      var param = new Dictionary<string, string>() { { "hoge", "fuga" } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value&hoge=fuga", url.Build(param));
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build());

      //パラメータ削除
      var array = new string[] { "key" };
      Assert.Equal("http://example.com/path/to/api?foo=bar", url.Build(array));
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build());

      var list = new List<string>() { "key" };
      Assert.Equal("http://example.com/path/to/api?foo=bar", url.Build(list));
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build());
    }
  }
}
