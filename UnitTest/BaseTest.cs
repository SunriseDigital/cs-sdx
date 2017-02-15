using System;

using UnitTest.DummyClasses;
using System.Text.RegularExpressions;

#if ON_VISUAL_STUDIO
using ClassInitialize = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using TestInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TestCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

using System.Threading;
using System.Globalization;
using System.Web;
using System.IO;
using System.Configuration;

namespace UnitTest
{
  public class Fixture : IDisposable
  {
    private BaseTest testClass;

    public void Dispose()
    {
      if(this.testClass != null)
      {
        this.testClass.FixtureTearDown();
      }
    }

    public BaseTest TestClass
    {
      set
      {
        if(this.testClass == null)
        {
          value.FixtureSetUp();
        }

        this.testClass = value;
      }
    }
  }
  public class BaseTest : Xunit.ICollectionFixture<Fixture>, IDisposable
  {
    public BaseTest()
    {
      Sdx.Context.Current.Timer.Start();
      Sdx.Context.Current.Debug.Out = Console.Out;
      //DB Adapter
      var settings = ConfigurationManager.GetSection("sdxDatabaseConnections") as Sdx.Configuration.DictionaryListSection;
      foreach (var elem in settings.Items)
      {
        Sdx.Db.Adapter.Manager.Add(elem.Attributes, ConfigurationManager.ConnectionStrings, ConfigurationManager.AppSettings);
      }

    }

    //public void SetFixture(Fixture fixture)
    //{
    //  fixture.TestClass = this;
    //  this.SetUp();
    //}

    [TestInitialize]
    public void TestInitialize()
    {
      this.SetUp();
    }

    [TestCleanup]
    public void TestCleanup()
    {
      this.TearDown();
    }
    
    virtual public void FixtureSetUp()
    {

    }
    
    virtual protected void SetUp()
    {
      Thread.CurrentThread.CurrentCulture = new CultureInfo("ja-JP");
    }

    virtual protected void TearDown()
    {

    }

    virtual public void FixtureTearDown()
    {

    }

    public void Dispose()
    {
#if !ON_VISUAL_STUDIO
      this.TearDown();
#endif
    }

    protected string HtmlLiner(string html)
    {
      html = html.Replace("\r", "").Replace("\n", "");

      Regex re = new Regex("> +<", RegexOptions.Singleline);
      html = re.Replace(html, "><");

      return html.Trim();
    }

    protected void InitHttpContextMock(string queryString)
    {
      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://wwww.example.com", queryString),
        new HttpResponse(new StringWriter())
      );

      Sdx.Context.Current.Timer.Start();
      Sdx.Context.Current.Debug.Out = Console.Out;
    }
  }
}
