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
  public class Html_Tag : BaseTest
  {
    [Fact]
    public void TestHtml()
    {
      var html = new Sdx.Html.Tag("div");
      Assert.Equal("<div></div>", html.Render());

      Assert.Equal("<div class=\"foobar\"></div>", html.Render(Sdx.Html.Attr.Create().AddClass("foobar")));
      Assert.Equal("<div></div>", html.Render());

      html.Attr.AddClass("foo");
      Assert.Equal("<div class=\"foo\"></div>", html.Render());
      Assert.Equal("<div class=\"foo bar\"></div>", html.Render(Sdx.Html.Attr.Create().AddClass("bar")));
      Assert.Equal("<div class=\"foo\"></div>", html.Render());

      html.AddHtml(new Sdx.Html.RawText("bar"));
      Assert.Equal("<div class=\"foo\">bar</div>", html.Render());

      var br = new Sdx.Html.VoidTag("br");
      Assert.Equal("<br>", br.Render());

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
      Assert.Equal(
        "<div><p>日本語<br>English<span class=\"foo\">span</span></p></div>",
        div.Render()
      );

      span = new Sdx.Html.Tag("span");
      span
        .AddText("foobar")
        .AddText("日本語");

      Assert.Equal(
        "<span>foobar日本語</span>",
        span.Render()
      );

      span = new Sdx.Html.Tag("span");
      span.AddText("\"&<>");
      Assert.Equal(
        "<span>&quot;&amp;&lt;&gt;</span>",
        span.Render()
      );
    }

    [Fact]
    public void TestIf()
    {
      var div = new Sdx.Html.Tag("div");
      div.If(h => h.TagName == "div", h => h.Attr.AddClass("disabled"), h => h.Attr.AddClass("enabled"));

      Assert.True(div.Attr.HasClass("disabled"));
      Assert.False(div.Attr.HasClass("enabled"));

      var span = new Sdx.Html.Tag("span");
      span.If(h => h.TagName == "div", h => h.Attr.AddClass("disabled"), h => h.Attr.AddClass("enabled"));

      Assert.False(span.Attr.HasClass("disabled"));
      Assert.True(span.Attr.HasClass("enabled"));
    }
  }
}
