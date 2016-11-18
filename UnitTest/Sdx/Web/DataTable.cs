using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using UnitTest.DummyClasses;
using Xunit;
using System.IO;

namespace UnitTest
{
  [TestClass]
  public class DeviceTable : BaseTest
  {
    [Fact]
    public void TestDeviceTable()
    {
      var fs = new FileStream("C:\\projects\\cs-sdx\\UnitTest\\config\\config.yml", FileMode.Open);
      var input = new StreamReader(fs, Encoding.GetEncoding("utf-8"));

      //decode
      var yaml = input.ReadToEnd();

      Sdx.Util.Yaml.Decode<dynamic>(yaml);

      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Util.Yaml.Decode<dynamic>(yaml));

      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/mb/yoshiwara/shop/?tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }
  }
}
