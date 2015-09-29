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
/*
    [Fact]
    public void TestMultipleParams()
    {
      Console.WriteLine("TestMethod1");
      //コンストラクタで、URLの分解・解析は済ませておく
      var url = new Sdx.Web.Url("http://example.com/path/to/api?foo=bar&hoge=huga");

      //コンストラクタの解析が意図通りに行われているか、各部品ごとに確認
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("bar", url.GetParam("foo"));
      Assert.Equal("huga", url.GetParam("hoge"));
      Assert.Equal("/path/to/api", url.LocalPath);

      //新しくパラメータを追加した場合、正しくクエリが生成されているか
      url.SetParam("key", "value");
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //Setterではなくコンストラクタの引数にパラメータを渡した場合の挙動
      //オブジェクトが持つデータ自体は変わらないようにする
      var param = new Dictionary<string, string>() { {"new", "newValue"} };
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

      //空の Dictionary を追加
      var empDic = new Dictionary<string, string>();
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build(empDic));

      //値が空になっている Dictionary を追加
      // * 値が空文字の場合
      var empValDic = new Dictionary<string, string>() { { "newkey", "" } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value&newkey=", url.Build(empValDic));

      // * 値が null の場合
      var nulValDic = new Dictionary<string, string>() { { "newkey", null } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value&newkey=", url.Build(nulValDic));

      //存在しないキーをList, Array で渡す
      //(パラメータがまだ何も無い状態で、特定のキーを削除しようとした場合の挙動チェックも兼ねて)
      var unknownList = new List<string>() { "unknown" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build(unknownList));

      var unknownArray = new string[] { "unknown" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build(unknownArray));

      //存在しないキーを指定して、想定した例外になっているかを確認する
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
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

      //パラメータの追加(メソッドで)
      url.SetParam("key", "value");
      Assert.Equal("value", url.GetParam("key"));
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

      //空の Dictionary を追加
      var empDic = new Dictionary<string, string>();
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build(empDic));

      //値が空になっている Dictionary を追加
      // * 値が空文字の場合
      var empValDic = new Dictionary<string, string>() { { "newkey", "" } };
      Assert.Equal("http://example.com/path/to/api?key=value&newkey=", url.Build(empValDic));

      // * 値が null の場合
      var nulValDic = new Dictionary<string, string>() { { "newkey", null } };
      Assert.Equal("http://example.com/path/to/api?key=value&newkey=", url.Build(nulValDic));

      //存在しないキーをList, Array で渡す
      //(パラメータがまだ何も無い状態で、特定のキーを削除しようとした場合の挙動チェックも兼ねて)
      var unknownList = new List<string>() { "unknown" };
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build(unknownList));

      var unknownArray = new string[] { "unknown" };
      Assert.Equal("http://example.com/path/to/api?key=value", url.Build(unknownArray));

      //存在しないキーを指定して、想定した例外になっているかを確認する
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
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

      //パラメータの追加(メソッドで)
      url.SetParam("key", "value");
      Assert.Equal("value", url.GetParam("key"));
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

      //空の Dictionary を追加
      var empDic = new Dictionary<string, string>();
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build(empDic));

      //値が空になっている Dictionary を追加
      // * 値が空文字の場合
      var empValDic = new Dictionary<string, string>() { { "newkey", "" } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value&newkey=", url.Build(empValDic));

      // * 値が null の場合
      var nulValDic = new Dictionary<string, string>() { { "newkey", null } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value&newkey=", url.Build(nulValDic));

      //存在しないキーをList, Array で渡す
      //(パラメータがまだ何も無い状態で、特定のキーを削除しようとした場合の挙動チェックも兼ねて)
      var unknownList = new List<string>() { "unknown" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build(unknownList));

      var unknownArray = new string[] { "unknown" };
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=value", url.Build(unknownArray));

      //存在しないキーを指定して、想定した例外になっているかを確認する
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
    }
*/
    [Fact]
    public void TestSameKeyNameParams()
    {
      var url = new Sdx.Web.Url("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2");

      //パスの各部品を取得
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("/path/to/api", url.LocalPath);

      //List<string> でパラメータ取得
      var list = url.GetParamList("sameKey");
      Assert.Equal("value0", list[0]);
      Assert.Equal("value1", list[1]);
      Assert.Equal("value2", list[2]);

      //取得したListに値は追加してもBuild時に生成されるURL文字列に影響ないことを期待
      list.Add("addedValue");
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2", url.Build());

      //文字列でパラメータ取得。Listの最終要素が取得できることを期待
      var str = url.GetParam("sameKey");
      Assert.Equal("value2", str);
      //str = "addedStr";
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2", url.Build());

      //Setメソッドでパラメータ追加
      url.SetParam("newKey", "newValue");
      Assert.Equal("newValue", url.GetParam("newKey"));
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //Build()の引数でパラメータ追加
      //var tmpParam = new Dictionary<string, string>() { {"tmpKey", "tmpValue"} };
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&tmpKey=tmpValue", url.Build(tmpParam));
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //Build()の引数でパラメータ削除テスト
      //var exclude = new List<string>() { "sameKey" };
      //Assert.Equal("http://example.com/path/to/api?newKey=newValue", url.Build(exclude));
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //値が空文字の Dictionary を追加
      //var empValDic = new Dictionary<string, string>() { { "empKey", "" } };
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&empKey=", url.Build(empValDic));
      //Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //存在しないキーで値を取得しようとした場合、例外になる。
      //Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
    }
  }
}
