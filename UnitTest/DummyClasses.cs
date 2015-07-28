using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest.DummyClasses
{
  /// <summary>
  /// xunitをVisualStudioのテストエクスプローラーで動かし、なおかつtravisでコンパイルを通すために空のAttributeを作成。
  /// </summary>
  class TestClassAttribute : Attribute { }

  class ClassInitializeAttribute : Attribute { }

  class ClassCleanupAttribute : Attribute { }

  class TestInitializeAttribute : Attribute { }

  class TestCleanupAttribute : Attribute { }

  public class TestContext { }
}
