using System;
using System.Diagnostics; 

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
      this.TearDown();
    }
  }
}
