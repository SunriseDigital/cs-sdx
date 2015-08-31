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

      Assert.Equal("マルチバイト\"文字中\"のクオート", config.GetString("test.multi_string"));
      Assert.Equal("マルチバイト文字の複数行\nマルチバイト文字の二行目\n", config.GetString("test.bill-to.street"));
    }
  }
}
