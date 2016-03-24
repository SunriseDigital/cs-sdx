using Xunit;
using UnitTest.DummyClasses;
using Moq;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;

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
  public class Pager : BaseTest
  {
    [Fact]
    public void TestTotalCount()
    {
      var pager = new Sdx.Pager();
      pager.SetPageData("1", 10, 38);

      Assert.Equal(4, pager.LastPage);
      Assert.Equal(10, pager.PerPage);
      Assert.Equal(38, pager.TotalCount);
      Assert.Equal(1, pager.Page);
      Assert.True(pager.HasNext);
      Assert.False(pager.HasPrev);

      pager.SetPageData("2", 10, 38);
      Assert.Equal(2, pager.Page);
      Assert.True(pager.HasNext);
      Assert.True(pager.HasPrev);

      pager.SetPageData("4", 10, 38);
      Assert.Equal(4, pager.Page);
      Assert.False(pager.HasNext);
      Assert.True(pager.HasPrev);
    }

    [Fact]
    public void TestSimple()
    {
      var pager = new Sdx.Pager();
      pager.SetPageData("1", true);

      Assert.Equal(1, pager.Page);
      Assert.True(pager.HasNext);
      Assert.False(pager.HasPrev);

      Exception ex;
      ex = Record.Exception(() => 
      {
        Assert.Equal(4, pager.LastPage);
      });

      Assert.IsType<InvalidOperationException>(ex);

      ex = Record.Exception(() =>
      {
        Assert.Equal(4, pager.PerPage);
      });

      Assert.IsType<InvalidOperationException>(ex);

      ex = Record.Exception(() =>
      {
        Assert.Equal(4, pager.TotalCount);
      });

      Assert.IsType<InvalidOperationException>(ex);

      pager.SetPageData("5", false);

      Assert.Equal(5, pager.Page);
      Assert.False(pager.HasNext);
      Assert.True(pager.HasPrev);
    }
  }
}
