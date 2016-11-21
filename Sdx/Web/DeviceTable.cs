using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sdx.Web
{
  public class DeviceTable
  {
    private string pcUrl { get; set; }

    private string spUrl { get; set; }

    private string mbUrl { get; set; }

    private Dictionary<string, object> urlDic = new Dictionary<string, object>();

    public enum Device
    {
      Pc,
      Sp,
      Mb
    }

    public DeviceTable(YamlNode pageYaml)
    {
      foreach (var item in (YamlMappingNode)pageYaml)
      {
        urlDic.Add(item.Key.ToString(), item.Value);
      }
    }

    public static Sdx.Web.DeviceTable Current 
    {
      get
      {
        if(!Sdx.Context.Current.Vars.ContainsKey("Sdx.Web.DeviceTable.Current"))
        {
          Sdx.Context.Current.Vars["Sdx.Web.DeviceTable.Current"] = CreateCurrent();
        }

        return (Sdx.Web.DeviceTable)Sdx.Context.Current.Vars["Sdx.Web.DeviceTable.Current"];
      }
    }

    private static Sdx.Web.DeviceTable CreateCurrent()
    {
      var filePath = WebConfigurationManager.AppSettings["Sdx.Web.DeviceTable.SettingFilePath"];
      //if(filePath == null){
        
      //}

      using (FileStream fs = new FileStream(filePath, FileMode.Open))
      {
        using (var input = new StreamReader(fs, Encoding.GetEncoding("utf-8")))
        {
          var yaml = new YamlStream();
          yaml.Load(input);
          var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

          var yamlSettings = (YamlSequenceNode)mapping.Children[new YamlScalarNode("page")];          

          foreach (YamlMappingNode pageYaml in yamlSettings)
          {
            var deviceTable = new Sdx.Web.DeviceTable(pageYaml); 

            if (deviceTable.IsMatch(Device.Pc, HttpContext.Current.Request.Url.AbsolutePath))
            {
              return deviceTable;
            }
          }

          return null;
        }
      }
    }

    public bool IsMatch(Device device, string url)
    {
      foreach (var d in urlDic)
      {
        Console.WriteLine("Key = {0}, Value = {1}", d.Key, d.Value);
      }
      string checkUrl;
      if (device == Device.Pc)
      {
        checkUrl = pcUrl;
      }
      else if (device == Device.Sp)
      {
        checkUrl = spUrl;
      }
      else
      {
        checkUrl = mbUrl;
      }

      if (checkUrl == url)
      {
        return true;
      }

      return false;
    }

    public string GetUrl(Device device)
    {
      return "";
    }
  }
}
