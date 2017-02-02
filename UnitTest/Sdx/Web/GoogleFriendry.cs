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
using System.Web;
using System.IO;

namespace UnitTest
{
  [TestClass]
  public class Web_GoogleFriendry : BaseTest
  {
    private const string PC_USER_AGENT = "Mozilla/5.0 (MSIE 10.0; Windows NT 6.1; Trident/5.0)";
    private const string SP_USER_AGENT = "Mozilla/5.0 (iPhone; CPU iPhone OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5376e Safari/8536.25";
    private const string MB_USER_AGENT = "D502i	DoCoMo/1.0/D502i	DoCoMo/1.0/D502i/c10";

    [Fact]
    public void TestSimplePcUrl()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/top.aspx", "");
      
      var gfriend = new Sdx.Web.GoogleFriendry(mb: "/i/top.aspx", sp: "/sp/top.aspx");
      Assert.True(gfriend.IsPcUrl);
      Assert.False(gfriend.IsMbUrl);
      Assert.False(gfriend.IsSpUrl);

      Assert.Equal("/top.aspx", gfriend.PcUrl.Build());
      Assert.Equal("/i/top.aspx", gfriend.MbUrl.Build());
      Assert.Equal("/sp/top.aspx", gfriend.SpUrl.Build());

      Assert.Equal(null, gfriend.RedirectUrl);

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(SP_USER_AGENT);
      Assert.Equal("/sp/top.aspx", gfriend.RedirectUrl.Build());

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(MB_USER_AGENT);
      Assert.Equal("/i/top.aspx", gfriend.RedirectUrl.Build());
    }

    [Fact]
    public void TestSimpleSpUrl()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/sp/top.aspx", "");

      var gfriend = new Sdx.Web.GoogleFriendry(pc: "/top.aspx", mb: "/i/top.aspx");
      Assert.False(gfriend.IsPcUrl);
      Assert.False(gfriend.IsMbUrl);
      Assert.True(gfriend.IsSpUrl);

      Assert.Equal("/top.aspx", gfriend.PcUrl.Build());
      Assert.Equal("/i/top.aspx", gfriend.MbUrl.Build());
      Assert.Equal("/sp/top.aspx", gfriend.SpUrl.Build());

      Assert.Equal("/top.aspx", gfriend.RedirectUrl.Build());

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(SP_USER_AGENT);
      Assert.Equal(null, gfriend.RedirectUrl);

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(MB_USER_AGENT);
      Assert.Equal("/i/top.aspx", gfriend.RedirectUrl.Build());
    }

    [Fact]
    public void TestSimpleMbUrl()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/i/top.aspx", "");

      var gfriend = new Sdx.Web.GoogleFriendry(sp: "/sp/top.aspx", pc: "/top.aspx");
      Assert.False(gfriend.IsPcUrl);
      Assert.True(gfriend.IsMbUrl);
      Assert.False(gfriend.IsSpUrl);

      Assert.Equal("/top.aspx", gfriend.PcUrl.Build());
      Assert.Equal("/i/top.aspx", gfriend.MbUrl.Build());
      Assert.Equal("/sp/top.aspx", gfriend.SpUrl.Build());

      Assert.Equal("/top.aspx", gfriend.RedirectUrl.Build());

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(SP_USER_AGENT);
      Assert.Equal("/sp/top.aspx", gfriend.RedirectUrl.Build());

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(MB_USER_AGENT);
      Assert.Equal(null, gfriend.RedirectUrl);
    }

    [Fact]
    public void TestUrlPlaceFolder()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/base/foo2/bar5/top.aspx", "");

      var gfriend = new Sdx.Web.GoogleFriendry(
        mb: "/i/base/bar{2}/foo{1}/top.aspx",
        sp: "/sp/bar{2}/foo{1}/top.aspx",
        regex: "^/base/foo([0-9]+?)/bar([0-9]+?)/top.aspx"
      );
      Assert.Equal("/i/base/bar5/foo2/top.aspx", gfriend.MbUrl.Build());
      Assert.Equal("/sp/bar5/foo2/top.aspx", gfriend.SpUrl.Build());
    }

    [Fact]
    public void TestSameQuery()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/?foo=1&bar=13&hoga=8", "foo=1&bar=13&hoga=8");

      var gfriend = new Sdx.Web.GoogleFriendry(
        mb: "/i/",
        sp: "/sp/"
      );
      Assert.Equal("/i/?foo=1&bar=13&hoga=8", gfriend.MbUrl.Build());
      Assert.Equal("/sp/?foo=1&bar=13&hoga=8", gfriend.SpUrl.Build());

      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/m10/s12/?foo=1&bar=13&hoga=8", "foo=1&bar=13&hoga=8");
      gfriend = new Sdx.Web.GoogleFriendry(
        mb: "/i/{1}/{2}/",
        sp: "/sp/{1}/{2}/",
        regex: "^/(m[0-9]+?)/(s[0-9]+?)/"
      );
      Assert.Equal("/i/m10/s12/?foo=1&bar=13&hoga=8", gfriend.MbUrl.Build());
      Assert.Equal("/sp/m10/s12/?foo=1&bar=13&hoga=8", gfriend.SpUrl.Build());
    }

    [Fact]
    public void TestDifferentQuery()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/?foo=1&bar=13&hoga=8", "foo=1&bar=13&hoga=8");

      var gfriend = new Sdx.Web.GoogleFriendry(
        mb: "/i/",
        sp: "/sp/"
      );
      gfriend.AddMbQueryMap("foo", "mfoo");
      gfriend.AddMbQueryMap("hoga", "mhoga");
      gfriend.AddSpQueryMap("foo", "sfoo");
      gfriend.AddSpQueryMap("hoga", "shoga");
      Assert.Equal("/i/?mfoo=1&bar=13&mhoga=8", gfriend.MbUrl.Build());
      Assert.Equal("/sp/?sfoo=1&bar=13&shoga=8", gfriend.SpUrl.Build());

      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/m10/s12/?foo=1&bar=13&hoga=8", "foo=1&bar=13&hoga=8");
      gfriend = new Sdx.Web.GoogleFriendry(
        mb: "/i/{1}/{2}/",
        sp: "/sp/{1}/{2}/",
        regex: "^/(m[0-9]+?)/(s[0-9]+?)/"
      );
      gfriend.AddMbQueryMap("foo", "mfoo");
      gfriend.AddMbQueryMap("hoga", "mhoga");
      gfriend.AddSpQueryMap("foo", "sfoo");
      gfriend.AddSpQueryMap("hoga", "shoga");
      Assert.Equal("/i/m10/s12/?mfoo=1&bar=13&mhoga=8", gfriend.MbUrl.Build());
      Assert.Equal("/sp/m10/s12/?sfoo=1&bar=13&shoga=8", gfriend.SpUrl.Build());
    }
  }
}
