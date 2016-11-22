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
    private Dictionary<string, object> settings = new Dictionary<string, object>();

    private Dictionary<string, string> regex = new Dictionary<string, string>();

    private Dictionary<string, string> queries = new Dictionary<string, string>();

    private Dictionary<string, string> urls = new Dictionary<string, string>();

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
          Dictionary<string, object> values = new Dictionary<string, object>();
          foreach (var value in (YamlMappingNode)item.Value)
          {            
            if (value.Value is YamlScalarNode)
            {
              values.Add(value.Key.ToString(), value.Value.ToString());
            }
            else
            {
              values.Add(value.Key.ToString(), value.Value);
            }            
          }
          settings.Add(item.Key.ToString(), values);        
        }
      }

      foreach (var item in settings)
      {
        foreach (var child in (Dictionary<string, object>)item.Value)
        {          
          if (child.Key.ToString() == "query")
          {
            foreach (var query in (YamlMappingNode)child.Value)
            {
              queries.Add(item.Key.ToString(), query.Value.ToString());
            }
          }
          else if (child.Key.ToString() == "query")
          {
            foreach (var url in (YamlMappingNode)child.Value)
            {
              urls.Add(item.Key.ToString(), url.Value.ToString());
            }
          }
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

      string[] settingPath = urls["pc"].ToString().Split('/');
            
      //Regex reg = new Regex(@"(yoshiwara|kanagawa)");
      //Match m = reg.Match(url);
      
      //if (m.Success)
      //{
      //  Console.WriteLine("{0,-10} : {1}", m.Value, m.Result("{area:$0}"));
      //}

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
  }
}
