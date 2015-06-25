using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnitTest.Attibute
{
  /// <summary>
  /// xunitをVisualStudioのテストエクスプローラーで動かし、なおかつtravisでコンパイルを通すために空のAttributeを作成。
  /// </summary>
  class TestClassAttribute : Attribute
  {
  }
}
