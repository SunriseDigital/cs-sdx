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
using System.Collections;
using YamlDotNet.RepresentationModel;

namespace UnitTest
{
  [TestClass]
  public class DeviceTable : BaseTest
  {
    public dynamic yamlNode;

    [Fact]
    public void TestDeviceTable()
    {
      loadTestYaml();
      
      var deviceTable = Sdx.Web.DeviceTable.Current;

      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/mb/yoshiwara/shop/?tg_prices_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }

    [Fact]
    public void TestIsMatch()
    {
      loadTestYaml();

      var deviceTable = new Sdx.Web.DeviceTable(yamlNode);

      Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1"));
      Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_prices_high=1"));
      Assert.True(deviceTable.IsMatch(Sdx.Web.DeviceTable.Device.Mb, "/mb/yoshiwara/shop/?tg_prices_high=1"));
    }

    private void loadTestYaml()
    {
      using (FileStream fs = new FileStream("C:\\projects\\cs-sdx\\UnitTest\\config\\config.yml", FileMode.Open))
      {
        using (var input = new StreamReader(fs, Encoding.GetEncoding("utf-8")))
        {
          var yaml = new YamlStream();
          yaml.Load(input);
          var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

          var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("page")];
          Dictionary<string, object> urlDic = new Dictionary<string, object>();
          foreach (YamlMappingNode item in items)
          {
            yamlNode = item;

            foreach (var child in item)
            {
              if (!urlDic.ContainsKey(child.Key.ToString()))
              {
                urlDic.Add(child.Key.ToString(), child.Value);
              }
            }
          }

          foreach (var d in urlDic)
          {
            Console.WriteLine("Key = {0}, Value = {1}", d.Key, d.Value);
          }
        }
      }
    }
  }
}
