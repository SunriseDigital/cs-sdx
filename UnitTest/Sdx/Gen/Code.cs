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
using System.Linq;
using System.Text.RegularExpressions;

namespace UnitTest
{
  [TestClass]
  public class Gen_Code : BaseTest
  {
    [Fact]
    public void SimpleStatement()
    {
      var code = new Sdx.Gen.Code.File();

      code.AddChild("using {0};", "System");
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
      code.AddChild("using {0};", "System");
      code.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace {0}", "UnitTest");
      code.AddChild(bNamespace);

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
      code.AddChild("using {0};", "System");
      code.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace UnitTest");
      code.AddChild(bNamespace);

      var className = "Test";
      var bClass = new Sdx.Gen.Code.Block("public class {0} : Base", className);
      bNamespace.AddChild(bClass);

      var bCtor = new Sdx.Gen.Code.Block("public {0}()", className);
      bClass.AddChild(bCtor);
      
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

    [Fact]
    public void FindCode()
    {
      var code = new Sdx.Gen.Code.File();

      var sUsingSystem = new Sdx.Gen.Code.Statement("using {0};", "System");
      code.AddChild(sUsingSystem);
      code.AddBlankLine();

      var bNamespace = new Sdx.Gen.Code.Block("namespace UnitTest");
      code.AddChild(bNamespace);

      var className = "Test";

      var bClass = new Sdx.Gen.Code.Block("public class {0} : Base", className);
      bNamespace.AddChild(bClass);

      var sMemberName = new Sdx.Gen.Code.Statement("private string name;");
      bClass.AddChild(sMemberName);

      var sMemberState = new Sdx.Gen.Code.Statement("private int state;");
      bClass.AddChild(sMemberState);

      var bCtor = new Sdx.Gen.Code.Block("public {0}()", className);
      bClass.AddChild(bCtor);

      var sCtorLine1 = new Sdx.Gen.Code.Statement("this.name = {0};", new Sdx.Gen.Code.String("foobar"));
      bCtor.AddChild(sCtorLine1);

      var sCtorLine2 = new Sdx.Gen.Code.Statement("state = 1;");
      bCtor.AddChild(sCtorLine2);

      var bMethodGet = new Sdx.Gen.Code.Block("public string Get()");
      bClass.AddChild(bMethodGet);
      bMethodGet.AddChild("return name;");

      Assert.Equal(@"
using System;

namespace UnitTest
{
  public class Test : Base
  {
    private string name;
    private int state;
    public Test()
    {
      this.name = ""foobar"";
      state = 1;
    }
    public string Get()
    {
      return name;
    }
  }
}
".TrimStart(), code.Render());

      //色々さがす
      Assert.Equal(sUsingSystem, code.FindChild("using System"));
      Assert.Equal(bNamespace, code.FindChild("namespace UnitTest"));
      Assert.Equal(bClass, code.FindChild("namespace UnitTest").FindChild("class Test"));
      Assert.Equal(sMemberName, code.FindChild("namespace UnitTest").FindChild("class Test").FindChild("string name"));
      Assert.Equal(sMemberState, code.FindChild("namespace UnitTest").FindChild("class Test").FindChild("int state"));
      Assert.Equal(bCtor, code.FindChild("namespace UnitTest").FindChild("class Test").FindChild(" Test()"));
      Assert.Equal(sCtorLine1, code.FindChild("namespace UnitTest").FindChild("class Test").FindChild(" Test()").GetChild(0));
      Assert.Equal(sCtorLine2, code.FindChild("namespace UnitTest").FindChild("class Test").FindChild(" Test()").GetChild(1));
      Assert.Equal(bMethodGet, code.FindChild("namespace UnitTest").FindChild("class Test").FindChild(" Get()"));

      Assert.Equal(bNamespace, code.FindChild(new Regex("^namespace UnitTest$")));
    }
  }
}
