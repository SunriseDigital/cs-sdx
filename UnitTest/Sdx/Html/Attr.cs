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
  public class Html_Attr : BaseTest
  {
    [Fact]
    public void TestAttr()
    {
      var attr = Sdx.Html.Attr.Create();

      attr.AddClass("foo");
      Assert.Equal("class=\"foo\"", attr.Render());

      attr.AddClass("bar", "hoge", "huga");
      Assert.Equal("class=\"foo bar hoge huga\"", attr.Render());

      attr.RemoveClass("hoge", "huga");

      attr.Set("data-attr", "datavalue");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());

      attr.SetStyle("width", "100px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px;\"", attr.Render());

      attr.SetStyle("height", "200px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());

      attr.Set("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled", attr.Render());

      //temp add

      Assert.Equal(
        "class=\"foo bar hoge\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled",
        attr.Merge(Sdx.Html.Attr.Create().AddClass("hoge", "bar")).Render()
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 150px; height: 200px; border: 1px;\" disabled",
        attr.Merge(Sdx.Html.Attr.Create().SetStyle("border", "1px").SetStyle("width", "150px")).Render()
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"update\" style=\"width: 100px; height: 200px;\" disabled data-attr2=\"datavalue2\"",
        attr.Merge(Sdx.Html.Attr.Create().Set("data-attr", "update").Set("data-attr2", "datavalue2")).Render()
      );

      //Remove

      attr.Remove("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());

      attr.RemoveStyle("width");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"height: 200px;\"", attr.Render());

      attr.RemoveStyle("height");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());

      attr.RemoveClass("foo");
      Assert.Equal("class=\"bar\" data-attr=\"datavalue\"", attr.Render());

      attr.RemoveClass("bar");
      Assert.Equal("data-attr=\"datavalue\"", attr.Render());

      attr.Remove("data-attr");
      Assert.Equal("", attr.Render());
    }
  }
}
