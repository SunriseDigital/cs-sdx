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
    public void SimpleStatement()
    {
      var code = new Sdx.Gen.Code.File();

      code.Add("using {0};", "System");
      Assert.Equal(@"
using System;
".TrimStart(), code.Render());

      code.AddBlankLine();
      Assert.Equal(@"
using System;

".TrimStart(), code.Render());
    }

    [Fact]
    public void SimpleBlock()
    {
      var code = new Sdx.Gen.Code.File();
      code.Add("using {0};", "System");
      code.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace {0}", "UnitTest");
      code.Add(bNamespace);

      Assert.Equal(@"
using System;

namespace UnitTest
{
}
".TrimStart(), code.Render());
    }

    [Fact]
    public void NestedBlock()
    {
      var code = new Sdx.Gen.Code.File();
      code.Add("using {0};", "System");
      code.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace UnitTest");
      code.Add(bNamespace);

      var className = "Test";
      var bClass = new Sdx.Gen.Code.Block("public class {0} : Base", className);
      bNamespace.Add(bClass);

      var bCtor = new Sdx.Gen.Code.Block("public {0}()", className);
      bClass.Add(bCtor);
      
      Assert.Equal(@"
using System;

namespace UnitTest
{
  public class Test : Base
  {
    public Test()
    {
    }
  }
}
".TrimStart(), code.Render());
    }
  }
}
