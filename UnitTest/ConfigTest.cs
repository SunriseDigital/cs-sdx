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

using System.IO;
using System.Collections.Generic;

namespace UnitTest
{
  [TestClass]
  public class ConfigTest : BaseTest
  {
    [Fact]
    public void TestYaml()
    {
      Sdx.Config.Tree config = new Sdx.Config.TreeYaml();
      config.BaseDir = Path.GetFullPath(".") + Path.DirectorySeparatorChar + "config";

      Assert.Equal("Oz-Ware Purchase Invoice", config.Get("test.receipt").Value);

      var strDic = config.Get("test.customer");
      Assert.Equal("Dorothy", strDic.Get("given").Value);

      var strList = config.Get("test.list");
      Assert.Equal(3, strList.Count);
      Assert.Equal("foo", strList[0].Value);
      Assert.Equal("bar", strList[1].Value);
      Assert.Equal("foobar", strList[2].Value);
      foreach (var item in strList)
      {
        //foreachできるかどうかのテストなので、何もAssertしない。
      }

      Assert.Equal("マルチバイト\"文字中\"のクオート", config.Get("test.multi-string").Value);
      Assert.Equal("マルチバイト文字の複数行\nマルチバイト文字の二行目\n", config.Get("test.bill-to.street").Value);

      var nestedDic = config.Get("test.nested-dic");
      Assert.Equal("普通の文字列", nestedDic.Get("plane-string").Value);
      Assert.Equal("value1", nestedDic.Get("inner-dic.key1").Value);
      Assert.Equal("value2", nestedDic.Get("inner-dic.key2").Value);

      var innerList = nestedDic.Get("inner-str-list");
      Assert.Equal(2, innerList.Count);
      Assert.Equal("listval1", innerList[0].Value);
      Assert.Equal("listval2", innerList[1].Value);

      var innerDicList = nestedDic.Get("inner-dic-list");
      Assert.Equal(2, innerDicList.Count);
      Assert.Equal("value3", innerDicList[0].Get("list-dic-key1").Value);
      Assert.Equal("value4", innerDicList[0].Get("list-dic-key2").Value);
      Assert.Equal("value5", innerDicList[1].Get("list-dic-key1").Value);
      Assert.Equal("value6", innerDicList[1].Get("list-dic-key2").Value);
    }

    [Fact]
    public void TestAssertMemoryCache()
    {
      Sdx.Config.Tree config = new Sdx.Config.TreeYaml();
      config.BaseDir = Path.GetFullPath(".") + Path.DirectorySeparatorChar + "config";

      var testReceipt = config.Get("test.receipt");
      Assert.Equal(testReceipt, config.Get("test.receipt"));

      var nestedDic = config.Get("test.nested-dic");
      Assert.Equal(nestedDic, config.Get("test.nested-dic"));

      //経由先が別なのでキャッシュされたものとは違ってしまう。
      Assert.NotEqual(nestedDic.Get("inner-dic.key1"), config.Get("test.nested-dic.inner-dic.key1"));

      var clonedList = new List<Sdx.Config.Tree>();
      foreach (var tree in nestedDic.Get("inner-str-list"))
      {
        clonedList.Add(tree);
      }

      var index = 0;
      foreach (var tree in nestedDic.Get("inner-str-list"))
      {
        Assert.Equal(clonedList[index], tree);
        ++index;
      }
    }

    [Fact]
    public void TestDirectory()
    {
      Sdx.Config.Tree config = new Sdx.Config.TreeYaml();
      config.BaseDir = Path.GetFullPath(".") + Path.DirectorySeparatorChar + "config";

      Assert.Equal("hoge", config.Get("dir/foo.bar").Value);
    }

    [Fact]
    public void TestSubclass()
    {
      //test of `dir`
      var config = new Test.Config.Dir();
      Assert.True(config.BaseDir.EndsWith("config"+Path.DirectorySeparatorChar+"dir"));

      var fooConfig = config.Get("foo");
      Assert.Equal("hoge", fooConfig.Get("bar").Value);
      Assert.Equal("foo", fooConfig.Get("dic.key1").Value);
      Assert.Equal("bar", fooConfig.Get("dic.key2").Value);

      //test of `dir2`
      var config2 = new Test.Config.Dir2();
      Assert.True(config2.BaseDir.EndsWith("config" + Path.DirectorySeparatorChar + "dir2"));

      var fooConfig2 = config2.Get("foo");
      Assert.Equal("hoge2", fooConfig2.Get("bar").Value);
      Assert.Equal("foo2", fooConfig2.Get("dic.key1").Value);
      Assert.Equal("bar2", fooConfig2.Get("dic.key2").Value);
    }
  }
}
