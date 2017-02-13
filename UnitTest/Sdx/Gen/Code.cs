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

namespace UnitTest
{
  [TestClass]
  public class Gen_Code : BaseTest
  {
    [Fact]
    public void SimpleBlock()
    {
      var code = new Sdx.Gen.Code.File();

      code.Add("using System;");
      Assert.Equal(@"
using System;
".TrimStart(), code.Render());

      code.AddLineBreak();
      Assert.Equal(@"
using System;

".TrimStart(), code.Render());

      var block = new Sdx.Gen.Code.Block("namespace UnitTest");
      code.Add(block);

      Assert.Equal(@"
using System;

namespace UnitTest
{
}
".TrimStart(), code.Render());
    }
  }
}
