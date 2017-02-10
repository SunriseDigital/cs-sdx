using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml;

namespace Sdx.Cli.Options
{
  public class Db
  {
    public enum AdapterType
    {
      sqlserver,
      mysql
    }

    public static void SetUpAdapters(IDbConnection options)
    {
      ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();

      fileMap.ExeConfigFilename = options.ConfigPath;
      System.Configuration.Configuration config =
          ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

      var sec = config.GetSection("sdxDatabaseConnections");
      XmlDocument doc = new XmlDocument();
      doc.Load(XmlReader.Create(new StringReader(sec.SectionInformation.GetRawXml())));
      foreach (XmlElement elem in doc["sdxDatabaseConnections"]["Items"])
      {
        var connection = new Dictionary<string, string>();
        foreach(XmlAttribute attr in elem.Attributes)
        {
          connection[attr.Name] = attr.Value;
        }

        Sdx.Db.Adapter.Manager.Set(connection, config.ConnectionStrings.ConnectionStrings, config.AppSettings);
      }
    }
  }
}
