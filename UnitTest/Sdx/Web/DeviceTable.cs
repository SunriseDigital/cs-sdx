#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

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
using System.Collections;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Web;
using System.Reflection;

namespace UnitTest
{
  [TestClass]
  public class DeviceTable : BaseTest
  {
    public dynamic yamlNode;

    [Fact]
    public void TestDeviceTable()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1&button=on", "../../config/config.yml");
      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?button=on&tg_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/m/yoshiwara/shop/?page=on&tg_price=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }

    [Fact]
    public void TestDeviceTable2()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_high=1&page=on&m=5", "../../config/config2.yml");
      Assert.Equal("/yoshiwara/shop/?button=on&m=5&tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?tg_high=1&page=on&m=5", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/m/yoshiwara/shop/?p=on&m=5&tg_price=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }

    //[Fact]
    //public void TestIsMatch()
    //{
    //  loadTestYaml("../../config/config.yml");

    //  var deviceTable = new Sdx.Web.DeviceTable(yamlNode);

    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/tmp/?tg_prices_high=1"));
    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1&button=on"));
    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1&button=off"));
    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_prices_high=1"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_high=1&button=on"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Mb, "/m/yoshiwara/shop/?tg_price=1&page=on"));      
    //}

    //[Fact]
    //public void TestIsMatch2()
    //{
    //  //query_matchが空の時
    //  loadTestYaml("../../config/config2.yml");

    //  var deviceTable = new Sdx.Web.DeviceTable(yamlNode);

    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/tmp/?tg_prices_high=1"));
    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1&button=on"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1&button=off"));
    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_high=1"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_high=1&button=on&page=on"));
    //  Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_high=1&page=on"));
    //  Assert.False(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Mb, "/m/yoshiwara/shop/?tg_price=1&button=on"));  
    //}

    private void loadTestYaml(string filePath)
    {
      using (FileStream fs = new FileStream(filePath, FileMode.Open))
      {
        using (var input = new StreamReader(fs, Encoding.GetEncoding("utf-8")))
        {
          var yaml = new YamlStream();
          yaml.Load(input);
          var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

          var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("page")];
          foreach (YamlMappingNode item in items)
          {
            yamlNode = item;
          }
        }
      }
    }
  }
}
