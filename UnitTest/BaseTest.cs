using System;

using UnitTest.DummyClasses;

#if ON_VISUAL_STUDIO
using ClassInitialize = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using TestInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TestCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

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
  public class BaseTest : Xunit.IUseFixture<Fixture>, IDisposable
  {
    public void SetFixture(Fixture fixture)
    {
      fixture.TestClass = this;
      this.SetUp();
    }

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
  }
}
