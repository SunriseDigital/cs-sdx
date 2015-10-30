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

      attr.AddClass("bar", "hoge", "huga");
      Assert.Equal("class=\"foo bar hoge huga\"", attr.Render());

      attr.RemoveClass("hoge", "huga");

      attr.Set("data-attr", "datavalue");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());

      attr.SetStyle("width", "100px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px;\"", attr.Render());

      attr.SetStyle("height", "200px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());

      attr.Add("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", attr.Render());

      //temp add

      Assert.Equal(
        "class=\"foo bar hoge\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"",
        attr.Render(Sdx.Html.Attr.Create().AddClass("hoge", "bar"))
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 150px; height: 200px; border: 1px;\" disabled=\"disabled\"",
        attr.Render(Sdx.Html.Attr.Create().SetStyle("border", "1px").SetStyle("width", "150px"))
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"update\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\" data-attr2=\"datavalue2\"",
        attr.Render(Sdx.Html.Attr.Create().Set("data-attr", "update").Set("data-attr2", "datavalue2"))
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

    [Fact]
    public void TestHtml()
    {
      Sdx.Html.Tag html = null;

      html = new Sdx.Html.Tag("div");
      Assert.Equal("<div></div>", html.Render());

      html.Attr.AddClass("foo");
      Assert.Equal("<div class=\"foo\"></div>", html.Render());

      html.AddHtml(new Sdx.Html.RawText("bar"));
      Assert.Equal("<div class=\"foo\">bar</div>", html.Render());

      html = new Sdx.Html.VoidTag("br");
      Assert.Equal("<br>", html.Render());

      var div = new Sdx.Html.Tag("div");
      var p = new Sdx.Html.Tag("p");
      div.AddHtml(p);

      p.AddHtml(new Sdx.Html.RawText("日本語"));
      p.AddHtml(new Sdx.Html.VoidTag("br"));
      p.AddHtml(new Sdx.Html.RawText("English"));

      var span = new Sdx.Html.Tag("span");
      span.AddHtml(new Sdx.Html.RawText("span"));
      span.Attr.AddClass("foo");
      p.AddHtml(span);

      Sdx.Context.Current.Debug.Log(div.Render());


    }
  }
}
