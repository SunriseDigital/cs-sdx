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
      config.BaseDir = Path.GetFullPath(".") + @"\config";

      Assert.Equal("Oz-Ware Purchase Invoice", config.Get<string>("test.receipt"));
      Assert.Equal("2007/08/06", config.Get<DateTime>("test.date").ToString("yyyy/MM/dd"));

      var strDic = config.Get<Dictionary<string, string>>("test.customer");
      Assert.Equal("Dorothy", strDic["given"]);

      var treeList = config.Get<List<Sdx.Config.Tree>>("test.items");
      Assert.Equal(2, treeList.Count);
      Assert.Equal("A4786", treeList[0].Get<string>("part_no"));

      var strList = config.Get<List<string>>("test.list");
      Assert.Equal(3, strList.Count);
      Assert.Equal("foo", strList[0]);
      Assert.Equal("bar", strList[1]);
      Assert.Equal("foobar", strList[2]);

    }
  }
}
