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
      config.BaseDir = Path.GetFullPath(".") + Path.DirectorySeparatorChar +"config";

      Assert.Equal("Oz-Ware Purchase Invoice", config.GetString("test.receipt"));
      //Assert.Equal("2007/08/06", config.Get<DateTime>("test.date").ToString("yyyy/MM/dd"));

      var strDic = config.GetStrDic("test.customer");
      Assert.Equal("Dorothy", strDic["given"]);

      var treeList = config.GetTreeList("test.items");
      Assert.Equal(2, treeList.Count);
      Assert.Equal("A4786", treeList[0].GetString("part_no"));

      var strList = config.GetStrList("test.list");
      Assert.Equal(3, strList.Count);
      Assert.Equal("foo", strList[0]);
      Assert.Equal("bar", strList[1]);
      Assert.Equal("foobar", strList[2]);

      Assert.Equal("マルチバイト\"文字中\"のクオート", config.GetString("test.multi-string"));
      Assert.Equal("マルチバイト文字の複数行\nマルチバイト文字の二行目\n", config.GetString("test.bill-to.street"));

      var nestedDic = config.GetTreeDic("test.nested-dic");
      Assert.Equal("普通の文字列", nestedDic["plane-string"].Value);
      Assert.Equal("value1", nestedDic["inner-dic"].GetString("key1"));
      Assert.Equal("value2", nestedDic["inner-dic"].GetString("key2"));

      var innerList = nestedDic["inner-str-list"].StrListValue;
      Assert.Equal(2, innerList.Count);
      Assert.Equal("listval1", innerList[0]);
      Assert.Equal("listval2", innerList[1]);

      var innerDicList = nestedDic["inner-dic-list"].List;
      Assert.Equal(2, innerDicList.Count);
      Assert.Equal("value3", innerDicList[0].GetString("list-dic-key1"));
      Assert.Equal("value4", innerDicList[0].GetString("list-dic-key2"));
      Assert.Equal("value5", innerDicList[1].GetString("list-dic-key1"));
      Assert.Equal("value6", innerDicList[1].GetString("list-dic-key2"));
    }

    [Fact]
    public void TestYaml2()
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
      foreach(var item in strList)
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
  }
}
