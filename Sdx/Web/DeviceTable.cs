using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    private Dictionary<string, object> deviceDic = new Dictionary<string, object>();

    private Dictionary<string, string> regex = new Dictionary<string, string>();

    Dictionary<string, string> queries = new Dictionary<string, string>();

    public enum Device
    {
      Pc,
      Sp,
      Mb
    }

    public DeviceTable(YamlMappingNode pageYaml)
    {      
      foreach (var item in pageYaml)
      {        
        if (item.Value is YamlNode)
        {
          Dictionary<string, object> deviceUrl = new Dictionary<string, object>();
          foreach (var value in (YamlMappingNode)item.Value)
          {
            
            if (value.Value is YamlScalarNode)
            {
              deviceUrl.Add(value.Key.ToString(), value.Value.ToString());
            }
            else
            {
              deviceUrl.Add(value.Key.ToString(), value.Value);
            }            
          }
          deviceDic.Add(item.Key.ToString(), deviceUrl);        
        }
      }

      if (deviceDic.ContainsKey("query"))
      {
        foreach (var query in (YamlMappingNode)deviceDic["query"])
        {
          queries.Add(query.Key.ToString(), query.Value.ToString());
        }
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
      string[] splitUrl = url.Split('?');
      string[] path = splitUrl[0].Split('/');

      string[] settingPath = splitUrl[0].Split('/');
      //string[] settingPath = deviceDic["url"].ToString().Split('/');
            
      Regex reg = new Regex(@"(yoshiwara|kanagawa)");
      Match m = reg.Match(url);
      
      if (m.Success)
      {
        //Console.WriteLine("{0,-10} : {1}", m.Value, m.Result("{area:$0}"));
      }

      queryMatch(splitUrl[1].Split('&'));

      //Console.WriteLine(settingPath.Any(x => path.Contains(x)));

      return settingPath.All(x => path.Contains(x));
    }

    private bool queryMatch(string[] splitQuery)
    {
      foreach (var query in splitQuery)
      {

      }

      return false;
    }

    public string GetUrl(Device device)
    {
      return "";
    }

    private string ObtainStatus(Device value)
    {
      string[] values = { "pc", "sp", "mb" };
      return values[(int)value];
    }
  }
}
