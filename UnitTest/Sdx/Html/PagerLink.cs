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
using System.Collections.Generic;

namespace UnitTest
{
  [TestClass]
  public class Html_PagerLink : BaseTest
  {
    [Fact]
    public void TestLinksTag()
    {
      InitHttpContextMock("pid=11");
      var pager = new Sdx.Pager(10, 200);
      pager.SetPage("11");
      var baseUrl = new Sdx.Web.Url("/path/to/target/page");
      var pagerLink = new Sdx.Html.PagerLink(pager, baseUrl, "pid");

      var links = pagerLink.GetLinksTag(5);
      Assert.Equal("<span class=\"current_page\">11</span>", links[2].Render());
      Assert.Equal("<a href=\"/path/to/target/page?pid=12\">12</a>", links[3].Render());
    }

    [Fact]
    public void TestLinksTagCallBack()
    {
      InitHttpContextMock("pid=11");
      var pager = new Sdx.Pager(10, 200);
      pager.SetPage("11");
      var baseUrl = new Sdx.Web.Url("/path/to/target/page");
      var pagerLink = new Sdx.Html.PagerLink(pager, baseUrl, "pid");

      var links = pagerLink.GetLinksTag(5, page => string.Format("<b>{0}</b>", page));
      Assert.Equal("<span class=\"current_page\"><b>11</b></span>", links[2].Render());
      Assert.Equal("<a href=\"/path/to/target/page?pid=12\"><b>12</b></a>", links[3].Render());
    }
  }
}
