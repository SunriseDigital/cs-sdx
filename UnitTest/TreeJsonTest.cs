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
using System.Diagnostics;
using Sdx;
using System.Collections;
using Sdx.Scaffold.Config;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnitTest
{
  [TestClass]
  public class TreeJsonTest : BaseTest
  {
    [Fact]
    public void TestTreeJson()
    {
      var fs = new FileStream("../../config/test.json", FileMode.Open);
      var input = new StreamReader(fs, Encoding.GetEncoding("utf-8"));
      Sdx.Data.Tree tree = new Sdx.Data.TreeJson();
      tree.Load(input);

      Assert.Equal("hoge", tree.Get("hoge").Value);
      Assert.Equal("orange", tree.Get("level1").Get("level2").Get("apple").Value);

      //var list = tree.Get("level1").Get("array");
      //Assert.Equal("12", list[0].Value);

      //var jobs = tree.Get("jobs");
      //Assert.Equal("0", jobs[0].Get("id").Value);

      Assert.False(tree.Get("level1").Get("level2").Exsits("orange"));
      Assert.True(tree.Get("level1").Exsits("level2"));

      Assert.Equal("日本語", tree.Get("level1.level2").Get("japanese").Value);
      Assert.Equal("日本語", tree.Get("level1.level2.japanese").Value);

      Assert.False(tree.Exsits("level1.level2.english"));
      Assert.False(tree.Get("level1").Get("level2").Exsits("english"));

      Assert.False(tree.Exsits("level1.level3.japanese"));

      Assert.True(tree.Get("level1").Exsits("level2.japanese"));

      Assert.True(tree.Exsits("fuga.file"));

      Assert.False(tree.Exsits("level1.level2.japanese.aaa"));
    }
  }
}