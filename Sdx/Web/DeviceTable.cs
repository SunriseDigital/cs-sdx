using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace Sdx.Web
{
  public class DeviceTable
  {
    private Dictionary<string, string> replaceWords = new Dictionary<string, string>();

    private Dictionary<string, string> urls = new Dictionary<string, string>();

    private Dictionary<string, object> settings = new Dictionary<string, object>();

    private Dictionary<string, object> queryMatch = new Dictionary<string, object>();

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
        Dictionary<string, object> tmp = new Dictionary<string, object>();
        foreach (var child in (YamlMappingNode)item.Value)
        {
          tmp.Add(child.Key.ToString(), child.Value);
        }

        settings.Add(item.Key.ToString(), tmp);
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

    public static Sdx.Web.DeviceTable CreateCurrent()
    {
      var filePath = WebConfigurationManager.AppSettings["Sdx.Web.DeviceTable.SettingFilePath"];
      if(filePath == null){
        throw new InvalidOperationException("Not Exists Sdx.Web.DeviceTable.SettingFilePath");
      }

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

            Device device = getDevice(HttpContext.Current.Request.Url.AbsolutePath);
            if (deviceTable.IsMatch(device, HttpContext.Current.Request.Url.AbsolutePath))
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
      string settingUrl = null;

      Dictionary<string, string> queries = new Dictionary<string, string>();

      foreach (var item in (Dictionary<string, object>)settings[deviceString(device)])
      {
        if (item.Key.ToString() == "url")
        {
           settingUrl = item.Value.ToString();
        }
        
        if (item.Key.ToString() == "query")
        {
          foreach (var query in (YamlMappingNode)item.Value)
          {
            queries.Add(query.Key.ToString(), query.Value.ToString());
          }
        }
      }

      string[] splitUrl = url.Split('?');
      List<string> path = splitUrl[0].Split('/').ToList();

      List<string> settingPath = settingUrl.Split('/').ToList();

      if (queries.Count > 0 && splitUrl.Length <= 1)
      {
        return false;
      }

      if (path.Count != settingPath.Count)
      {
        return false;
      }

      var notEqualPaths = 
        path
          .Select((item, index) => new { Index = index, Value = item })
          .Where(item => !pathCheck(item.Value, settingPath[item.Index]));

      if (notEqualPaths.Count() > 0)
      {
        return false;
      }

      if (splitUrl.Length > 1 && !queryCheck(splitUrl[1].Split('&'), queries))
      {
        return false;
      }

      return true;
    }

    private bool pathCheck(string path, string setting)
    {
      if (path != setting)
      {
        Regex reg = new Regex(@"^{([a-zA-Z0-9]+):(.*)}$");
        Match match = reg.Match(setting);
        if (match.Success)
        {
          //置換用の変数確保
          if (!replaceWords.ContainsKey(match.Result("$1").ToString()))
          {
            Regex r = new Regex(match.Result("$2").ToString());
            Match m = r.Match(path);
            if (!m.Success)
            {
              return false;
            }
            replaceWords.Add(match.Result("$1").ToString(), path);
          }

          return true;
        }

        return false;
      }

      return true;
    }

    private bool queryCheck(string[] splitQuery, Dictionary<string, string> queries)
    {      
      if (settings.ContainsKey("query_match"))
      {
        queryMatch = (Dictionary<string, object>)settings["query_match"];
      }

      if (queries.Count < 1)
      {
        return false;
      }

      Dictionary<string, string> dic = splitQuery.ToDictionary(n => n.Split('=')[0], n => n.Split('=')[1]);
      foreach (var query in dic)
      {
        if (queries.ContainsValue(query.Key))
        {
          var key = queries.First(x => x.Value == query.Key).Key;
          if (queryMatch.ContainsKey(key) && queryMatch[key].ToString() != query.Value)
          {
            return false;
          }
        }
        else
        {
          return false;
        }
      }
   
      return true;
    }

    public string GetUrl(Device device)
    {
      string url = "";

      Dictionary<string, object> deviceSettings = (Dictionary<string, object>)settings[deviceString(device)];

      foreach (var item in deviceSettings)
      {
        if (item.Key.ToString() == "url")
        {
          url = item.Value.ToString();
        }
      }

      string pattern = @"{([a-zA-Z0-9]+):(.*)}";
      Regex reg = new Regex(pattern);
      Match match = reg.Match(url);

      if(match.Success){
        url = Regex.Replace(url, pattern, replaceWords[match.Result("$1").ToString()]);
      }

      Dictionary<string, string> queries = new Dictionary<string, string>();
      foreach (var item in deviceSettings)
      {
        if (item.Key.ToString() == "query")
        {
          foreach (var query in (YamlMappingNode)item.Value)
          {
            queries.Add(query.Key.ToString(), query.Value.ToString());
          }

          var strings = queries.Select(kvp => string.Format("{0}={1}", kvp.Value, queryMatch[kvp.Key]));
          string path = string.Join("&", strings);
          url = url + "?" + path;
        }
      }

      return url;
    }

    private static Device getDevice(string url)
    {
      string device = "pc";

      Regex regex = new Regex(@"^/(m|sp|i)/.*?$");
      Match m = regex.Match(url);
      if (m.Success)
      {
        device = m.Result("$1").ToString();
      }

      var deviceDic = new Dictionary<string, Device>()
      {
        {"pc", Device.Pc},
        {"sp", Device.Sp},
        {"m", Device.Mb},
        {"i", Device.Mb}
      };

      return deviceDic[device];
    }

    private static string deviceString(Device device)
    {
      string[] strings = { "pc", "sp", "mb"};
      return strings[(int)device];
    }
  }
}
