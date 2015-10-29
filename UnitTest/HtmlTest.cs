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
  public class HtmlTest : BaseTest
  {
    [Fact]
    public void TestAttr()
    {
      var attr = Sdx.Html.Attr.Create();

      attr.AddClass("foo");
      Assert.Equal("class=\"foo\"", attr.Render());

      attr.AddClass("bar");
      Assert.Equal("class=\"foo bar\"", attr.Render());

      attr.Set("data-attr", "datavalue");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());

      //attr.Remove("data-attr");
      //Assert.Equal("class=\"foo bar\"", attr.Render());

      //attr.RemoveClass("foo");
      //Assert.Equal("class=\"bar\"", attr.Render());

      //attr.RemoveClass("bar");
      //Assert.Equal("", attr.Render());

      attr.SetStyle("width", "100px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px;\"", attr.Render());

      attr.SetStyle("height", "200px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());

      attr.Add("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", attr.Render());

      //一度取っておく
      var clonedAttr = (Sdx.Html.Attr)attr.Clone();

      attr.Remove("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", clonedAttr.Render());

      attr.RemoveStyle("width");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"height: 200px;\"", attr.Render());
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", clonedAttr.Render());

      attr.RemoveStyle("height");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", clonedAttr.Render());

      attr.RemoveClass("foo");
      Assert.Equal("class=\"bar\" data-attr=\"datavalue\"", attr.Render());
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", clonedAttr.Render());

      attr.RemoveClass("bar");
      Assert.Equal("data-attr=\"datavalue\"", attr.Render());
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", clonedAttr.Render());

      attr.Remove("data-attr");
      Assert.Equal("", attr.Render());
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", clonedAttr.Render());
    }
  }
}
