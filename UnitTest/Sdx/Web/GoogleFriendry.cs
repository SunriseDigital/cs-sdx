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
    public void TestSimpleUrl()
    {
      //mockをセット
      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(PC_USER_AGENT);
      Sdx.Context.Current.Request = new HttpRequest("", "http://www.example.com/top.aspx", "");
      
      var gfriend = new Sdx.Web.GoogleFriendry(mb: "/i/top.aspx", sp: "/sp/top.aspx");
      Assert.True(gfriend.IsPcUrl);
      Assert.False(gfriend.IsMbUrl);
      Assert.False(gfriend.IsSpUrl);
      
      Assert.Null(gfriend.RedirectUrl);

      Assert.Equal("/top.aspx", gfriend.PcUrl);
      Assert.Equal("/i/top.aspx", gfriend.MbUrl);
      Assert.Equal("/sp/top.aspx", gfriend.SpUrl);

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(SP_USER_AGENT);
      Assert.Equal("/sp/top.aspx", gfriend.RedirectUrl);

      Sdx.Context.Current.UserAgent = new Sdx.Web.UserAgent(MB_USER_AGENT);
      Assert.Equal("/i/top.aspx", gfriend.RedirectUrl);
    }
  }
}
