using Xunit;
using UnitTest.DummyClasses;
using System.Collections.Generic;
using System.Diagnostics;

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
  public class ContextTest : BaseTest
  {
    [Fact]
    public void TestVar()
    {
      Sdx.Context context = Sdx.Context.Current;
      Assert.Equal(context, Sdx.Context.Current);

      context.SetVar("foo", "bar");
      Assert.Equal("bar", Sdx.Context.Current.GetVar("foo"));

      var shop = new Test.Orm.Shop();
      context.SetVar("shop", shop);
      Assert.Equal(shop, Sdx.Context.Current.GetVar<Test.Orm.Shop>("shop"));

      Exception ex = Record.Exception(() => context.GetVar("hoge"));
      Assert.Equal(typeof(KeyNotFoundException), ex.GetType());
    }

    [Fact]
    public void TestRequestTimer()
    {
      Assert.Equal(typeof(Stopwatch), Sdx.Context.Current.Timer.GetType());
      Assert.False(Sdx.Context.Current.Timer.IsRunning);
      Sdx.Context.Current.Timer.Start();
      Assert.True(Sdx.Context.Current.Timer.IsRunning);
    }
  }
}
