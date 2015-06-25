using Xunit;
using UnitTest.Attibute;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

using System;

namespace UnitTest
{
  [TestClass]
  public class TestTemplate
  {
    [Fact]
    public void TestMethod()
    {

    }
  }
}
