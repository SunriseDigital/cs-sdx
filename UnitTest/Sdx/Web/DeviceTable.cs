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
    [Fact]
    public void GetUrlQueryReplaceTest()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?tg_prices_high=1&button=on", "../../config/config.yml");      
      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?button=on&tg_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/m/yoshiwara/shop/?page=on&tg_price=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?button=on&tg_high=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/m/yoshiwara/shop/?page=on&tg_price=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }

    [Fact]
    public void GetUrlQueryNotReplaceTest()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_prices_high=1&button=on&m=5", "../../config/config2.yml");
      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc));
      Assert.Equal("/sp/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/m/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }

    [Fact]
    public void QueryMatchTest()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/tmp/?tg_prices_high=1", "../../config/config.yml");
      Assert.Empty(deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp)); //対応表にマッチしないので空になるはず

      //query_matchが空の時 
      deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/?prices_high=1&button=on&m=5&p=2", "../../config/config2.yml");
      Assert.Empty(deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp)); //対応表にマッチしないので空になるはず
    }

    [Fact]
    public void QueryMatchPerfectTest()
    {
      //perfect_match
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/kanagawa/gal/?flag=1&m=5&page=2", "../../config/config2.yml");
      Assert.Empty(deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp)); //完全一致でないので（対応表よりクエリーが多い）空になるはず

      deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/kanagawa/gal/?page=2", "../../config/config2.yml");
      Assert.Empty(deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp)); //完全一致でないので（対応表よりクエリーがすくない）空になるはず

      deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/kanagawa/gal/?flag=1&page=2", "../../config/config2.yml");
      Assert.Equal("/sp/kanagawa/gal/?flag=1&p=2", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp)); //完全一致してかつクエリーのキーが変わって返ってくるはず
    }

    [Fact]
    public void ExcludeBuildQueryTest()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Sp, "/sp/yoshiwara/shop/?tg_prices_high=1&button=on&m=5&p=1", "../../config/config2.yml");
      Assert.Equal("/yoshiwara/shop/?tg_prices_high=1&button=on&p=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Pc)); //mのみ除外される
      Assert.Equal("/sp/yoshiwara/shop/?tg_prices_high=1&button=on", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp)); //mとpが除外される
      Assert.Equal("/m/yoshiwara/shop/?tg_prices_high=1&button=on&p=1", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb)); //mのみ除外される
    }

    [Fact]
    public void QueryMatchEmptyTest()
    {
      var deviceTable = new Sdx.Web.DeviceTable(Sdx.Web.DeviceTable.Device.Pc, "/yoshiwara/shop/", "../../config/config2.yml");
      Assert.Equal("/sp/yoshiwara/shop/", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp));
      Assert.Equal("/m/yoshiwara/shop/", deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb));
    }
  }
}
