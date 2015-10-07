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
      Assert.Equal("bar", url.GetParam("foo"));
      Assert.Equal("huga", url.GetParam("hoge"));
      Assert.Equal("/path/to/api", url.LocalPath);

      //新しくパラメータを追加した場合、正しくクエリが生成されているか
      url.AddParam("key", "value");
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //Setterではなくコンストラクタの引数にパラメータを渡した場合の挙動
      //オブジェクトが持つデータ自体は変わらないようにする
      var param = new Dictionary<string, string>() { {"new", "newValue"} };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value&new=newValue", url.Build(param));
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value", url.Build());

      //同じキー名のパラメータを引数に渡した場合の挙動。
      var dic = new Dictionary<string, string>() { { "key", "newValue" } };
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=value&key=newValue", url.Build(dic));
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

      //空の Dictionary を追加。この場合は除くキーの指定が何もないので、引数なしBuildと何も変わらないことを期待
      var empDic = new Dictionary<string, string>();
      Assert.Equal(url.Build(empDic), url.Build());

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

      //存在しないキーを指定した場合の挙動
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
      Assert.Empty(url.GetParams("unknown"));

      //Set メソッドのテスト。同じ名前のキーがある場合は上書きされる
      url.SetParam("key", "newValue");
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga&key=newValue", url.Build());

      //RemoveParam のテスト
      url.RemoveParam("key");
      Assert.Equal("http://example.com/path/to/api?foo=bar&hoge=huga", url.Build());

      // has メソッドのテスト
      Assert.True(url.HasParam("foo"));
      Assert.False(url.HasParam("unknown"));
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
      url.AddParam("key", "value");
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

      //存在しないキーを指定した場合の挙動
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
      Assert.Empty(url.GetParams("unknown"));

      //Set メソッドのテスト。同じ名前のキーがある場合は上書きされる
      url.SetParam("key", "newValue");
      Assert.Equal("http://example.com/path/to/api?key=newValue", url.Build());

      //RemoveParam のテスト
      url.RemoveParam("key");
      Assert.Equal("http://example.com/path/to/api", url.Build());

      // has メソッドのテスト
      Assert.False(url.HasParam("foo"));
      Assert.False(url.HasParam("unknown"));
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
      url.AddParam("key", "value");
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

      //存在しないキーを指定した場合の挙動
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
      Assert.Empty(url.GetParams("unknown"));

      //Set メソッドのテスト。同じ名前のキーがある場合は上書きされる
      url.SetParam("key", "newValue");
      Assert.Equal("http://example.com/path/to/api?foo=bar&key=newValue", url.Build());

      //RemoveParam のテスト
      url.RemoveParam("foo");
      Assert.Equal("http://example.com/path/to/api?key=newValue", url.Build());

      // has メソッドのテスト
      Assert.True(url.HasParam("key"));
      Assert.False(url.HasParam("unknown"));
    }

    [Fact]
    public void TestSameKeyNameParams()
    {
      var url = new Sdx.Web.Url("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2");

      //パスの各部品を取得
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("/path/to/api", url.LocalPath);

      //List<string> でパラメータ取得
      var list = url.GetParams("sameKey");
      Assert.Equal("value0", list[0]);
      Assert.Equal("value1", list[1]);
      Assert.Equal("value2", list[2]);

      //取得したListに値は追加してもBuild時に生成されるURL文字列に影響ないことを期待
      list.Add("addedValue");
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2", url.Build());

      //文字列でパラメータ取得。Listの最初の要素の値が取得できることを期待
      var str = url.GetParam("sameKey");
      Assert.Equal("value0", str);
      str = "addedStr";
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2", url.Build());

      //Addメソッドでパラメータ追加
      url.AddParam("newKey", "newValue");
      Assert.Equal("newValue", url.GetParam("newKey"));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //Build()の引数でパラメータ追加
      var tmpParam = new Dictionary<string, string>() { {"tmpKey", "tmpValue"} };
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&tmpKey=tmpValue", url.Build(tmpParam));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //Build()の引数でパラメータ削除テスト(List)
      var excludeList = new List<string>() { "sameKey" };
      Assert.Equal("http://example.com/path/to/api?newKey=newValue", url.Build(excludeList));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //Build()の引数でパラメータ削除テスト(Array)
      var excludeArray = new string[] { "sameKey" };
      Assert.Equal("http://example.com/path/to/api?newKey=newValue", url.Build(excludeArray));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //値が空文字の Dictionary を追加
      var empValDic = new Dictionary<string, string>() { { "empKey", "" } };
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&empKey=", url.Build(empValDic));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue", url.Build());

      //存在しないキーを指定した場合の挙動
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
      Assert.Empty(url.GetParams("unknown"));

      //Addメソッドで同じキーのパラメータを追加。上書きはされず値が増えるだけ。取得は最初に見つかったほうを取得。
      url.AddParam("newKey", "newValue2");
      Assert.Equal("newValue", url.GetParam("newKey"));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&newKey=newValue2", url.Build());

      //Build()の引数で同じキーのパラメータ追加。Addと同様だが、引数なしBuild()の結果には影響しない
      var addParam = new Dictionary<string, string>() { { "newKey", "newValue3" } };
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&newKey=newValue2&newKey=newValue3", url.Build(addParam));
      Assert.Equal("http://example.com/path/to/api?sameKey=value0&sameKey=value1&sameKey=value2&newKey=newValue&newKey=newValue2", url.Build());

      //SetParam のテスト。同じキーの値が既にあったら全て削除してから新しい値をセットする
      url.SetParam("sameKey", "value3");
      Assert.Equal("http://example.com/path/to/api?newKey=newValue&newKey=newValue2&sameKey=value3", url.Build());

      //RemoveParam のテスト。同じキーの値が複数あったら、その全てを削除する
      url.RemoveParam("newKey");
      Assert.Equal("http://example.com/path/to/api?sameKey=value3", url.Build());

      // has メソッドのテスト
      Assert.True(url.HasParam("sameKey"));
      Assert.False(url.HasParam("unknown"));
    }

    [Fact]
    public void TestNullCharValueParams()
    {
      var url = new Sdx.Web.Url("http://example.com/path/to/api?key=&key=&key=&foo=&foo=&foo=bar");

      //Path
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("/path/to/api", url.LocalPath);

      //Param
      Assert.Equal("", url.GetParam("key"));
      Assert.Equal("", url.GetParam("foo"));

      //ParamList
      var keyList = url.GetParams("key");
      Assert.Equal("", keyList[0]);
      Assert.Equal("", keyList[1]);
      Assert.Equal("", keyList[2]);

      var fooList = url.GetParams("foo");
      Assert.Equal("", fooList[0]);
      Assert.Equal("", fooList[1]);
      Assert.Equal("bar", fooList[2]);

      //Null を Set
      url.SetParam("key", null);
      Assert.Equal("http://example.com/path/to/api?foo=&foo=&foo=bar&key=", url.Build());

      //空文字を Set
      url.SetParam("foo", "");
      Assert.Equal("http://example.com/path/to/api?key=&foo=", url.Build());

      //Null を Add
      url.AddParam("key", null);
      Assert.Equal("http://example.com/path/to/api?key=&foo=&key=", url.Build());

      //空文字を Add
      url.AddParam("foo", "");
      Assert.Equal("http://example.com/path/to/api?key=&foo=&key=&foo=", url.Build());

      // has メソッドのテスト
      Assert.True(url.HasParam("key"));
      Assert.True(url.HasParam("foo"));
      Assert.False(url.HasParam("unknown"));
    }

    [Fact]
    public void TestControlMultipleParams()
    {
      var url = new Sdx.Web.Url("http://example.com/path/to/api");

      //Path
      Assert.Equal("example.com", url.Domain);
      Assert.Equal("http", url.Scheme);
      Assert.Equal("/path/to/api", url.LocalPath);

      //テスト用パラメータ各種
      var paramsSet = new Dictionary<string, string>() {
        {"key", "value"},
        {"hoge", "fuga"},
        {"foo", "bar"}
      };
      var paramsEmp = new Dictionary<string, string>() {
        {"key", ""},
        {"hoge", ""},
        {"foo", ""}
      };
      var paramsAdd = new Dictionary<string, string>() {
        {"newKey", "newValue"},
        {"matsu", "take"}
      };

      //Build() で追加
      Assert.Equal("http://example.com/path/to/api?key=value&hoge=fuga&foo=bar", url.Build(paramsSet));
      Assert.Equal("http://example.com/path/to/api", url.Build());
      Assert.Equal("http://example.com/path/to/api?key=&hoge=&foo=", url.Build(paramsEmp));
      Assert.Equal("http://example.com/path/to/api", url.Build());

    　//Build() で削除
      var paramsRemove = new List<string>() { "key", "hoge", "foo" };
      url.SetParam("key", "value");
      url.SetParam("hoge", "fuga");
      url.SetParam("foo", "bar");
      Assert.Equal("http://example.com/path/to/api", url.Build(paramsRemove));
      Assert.Equal("http://example.com/path/to/api?key=value&hoge=fuga&foo=bar", url.Build());

      //has メソッド
      Assert.True(url.HasParam("key"));
      Assert.True(url.HasParam("hoge"));
      Assert.True(url.HasParam("foo"));
      Assert.False(url.HasParam("unknown"));

      //unknown key
      Assert.Throws<KeyNotFoundException>(() => url.GetParam("unknown"));
      Assert.Empty(url.GetParams("unknown"));
    }

    [Fact]
    public void TestVariousUrl()
    {
      //値に日本語、キーに日本語、エンコード済みの"&"、その他記号を含めた場合の挙動
      var url = new Sdx.Web.Url("http://example.com/path/to/テスト?key=価値&ほげ=f_u/g-a&multi=AAA%26BBB&kakko=【かっこ】");
      Assert.Equal("/path/to/テスト", url.LocalPath);
      Assert.Equal("価値", url.GetParam("key"));
      Assert.Equal("f_u/g-a", url.GetParam("ほげ"));
      Assert.Equal("AAA&BBB", url.GetParam("multi"));
      Assert.Equal("【かっこ】", url.GetParam("kakko"));
      Assert.Equal("http://example.com/path/to/%E3%83%86%E3%82%B9%E3%83%88?key=%E4%BE%A1%E5%80%A4&%E3%81%BB%E3%81%92=f_u/g-a&multi=AAA%26BBB&kakko=%E3%80%90%E3%81%8B%E3%81%A3%E3%81%93%E3%80%91", url.Build());

      //上記で Build() した エンコード済み URL を使った場合の挙動。エンコード済みのものはさらにエンコードはされない。
      url = new Sdx.Web.Url("http://example.com/path/to/%E3%83%86%E3%82%B9%E3%83%88?key=%E4%BE%A1%E5%80%A4&%E3%81%BB%E3%81%92=f_u/g-a&multi=AAA%26BBB&kakko=%E3%80%90%E3%81%8B%E3%81%A3%E3%81%93%E3%80%91");
      Assert.Equal("/path/to/テスト", url.LocalPath);
      Assert.Equal("価値", url.GetParam("key"));
      Assert.Equal("f_u/g-a", url.GetParam("ほげ"));
      Assert.Equal("AAA&BBB", url.GetParam("multi"));
      Assert.Equal("【かっこ】", url.GetParam("kakko"));
      Assert.Equal("http://example.com/path/to/%E3%83%86%E3%82%B9%E3%83%88?key=%E4%BE%A1%E5%80%A4&%E3%81%BB%E3%81%92=f_u/g-a&multi=AAA%26BBB&kakko=%E3%80%90%E3%81%8B%E3%81%A3%E3%81%93%E3%80%91", url.Build());
    }
  }
}
