using System;
using System.Collections;
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

    private Dictionary<string, object> settings = new Dictionary<string, object>();

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

    private static Sdx.Web.DeviceTable CreateCurrent()
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

            Device device = getDevice(HttpContext.Current.Request.RawUrl);
            if (deviceTable.IsMatch(device, HttpContext.Current.Request.RawUrl))
            {
              return deviceTable;
            }
          }

          return null;
        }
      }
    }

    public bool IsMatch(Device device, string currentUrl)
    {
      if (!settings.ContainsKey(deviceString(device)))
      {
        return false;
      }
      
      Dictionary<string, object> deviceSettings = (Dictionary<string, object>)settings[deviceString(device)];

      string[] splitUrl = currentUrl.Split('?');
      string[] currentPaths = splitUrl[0].Split('/');

      string[] settingPaths = deviceSettings["url"].ToString().Split('/');

      if (currentPaths.Length != settingPaths.Length)
      {
        return false;
      }

      var notEqualPaths =
        currentPaths
          .Select((item, index) => new { Index = index, Value = item })
          .Where(item => !pathCheck(item.Value, settingPaths[item.Index]));
      
      if (notEqualPaths.Count() > 0)
      {
        return false;
      }

      //query_matchがあったときだけ対応表のクエリを見る
      if (settings.ContainsKey("query_match"))
      {
        if (splitUrl.Length <= 1)
        {          
          return false;
        }

        Dictionary<string, string> currentQuery = splitUrl[1].Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);
        return queryCheck(currentQuery, deviceSettings);
      }

      return true;
    }

    private bool pathCheck(string currentPath, string settingPath)
    {
      if (currentPath != settingPath)
      {
        Regex reg = new Regex(@"^{([a-zA-Z0-9]+):(.*)}$");
        Match match = reg.Match(settingPath);
        if (match.Success)
        {
          //置換用の変数確保
          if (!replaceWords.ContainsKey(match.Result("$1").ToString()))
          {
            Regex r = new Regex(match.Result("$2").ToString());
            Match m = r.Match(currentPath);
            if (!m.Success)
            {
              return false;
            }
            replaceWords.Add(match.Result("$1").ToString(), currentPath);
          }

          return true;
        }

        return false;
      }

      return true;
    }

    private Dictionary<string, object> replaceQueryMatchKey(Dictionary<string, object> queryMatch, YamlMappingNode queries)
    {
      foreach (var query in queries)
      {
        if (queryMatch.ContainsKey(query.Key.ToString()))
        {
          queryMatch.Add(query.Value.ToString(), queryMatch[query.Key.ToString()]);
          queryMatch.Remove(query.Key.ToString());
        }
      }

      return queryMatch;
    }

    private bool queryCheck(Dictionary<string, string> currentQuery, Dictionary<string, object> deviceSettings)
    {
      Dictionary<string, object> queryMatch = (Dictionary<string, object>)settings["query_match"];
      YamlMappingNode settingQuery = null;

      if (deviceSettings.ContainsKey("query"))
      {
        settingQuery = (YamlMappingNode)deviceSettings["query"];
        queryMatch = replaceQueryMatchKey(queryMatch, settingQuery);
      }

      //先にquery_matchの処理
      foreach (var query in currentQuery)
      {
        if (queryMatch.ContainsKey(query.Key))
        {
          if (queryMatch[query.Key].ToString() != query.Value)
          {
            return false;
          }
        }
      }

      if (deviceSettings.ContainsKey("query"))
      {
        foreach (var item in (YamlMappingNode)deviceSettings["query"])
        {
          if (!currentQuery.ContainsKey(item.Value.ToString()))
          {
            return false;
          }
        }
      }
   
      return true;
    }

    public string GetUrl(Device device)
    {
      string url = "";

      if (settings.ContainsKey(deviceString(device)))
      {
        Dictionary<string, object> deviceSettings = (Dictionary<string, object>)settings[deviceString(device)];
        url = deviceSettings["url"].ToString();

        if (replaceWords.Count > 0)
        {
          foreach(var word in replaceWords)
          {
            string pattern = @"{([a-zA-Z0-9]+):(.*)}";
            Regex reg = new Regex(pattern);
            Match match = reg.Match(url);
            if (match.Success)
            {
              url = Regex.Replace(url, pattern, replaceWords[match.Result("$1").ToString()]);
            }
          }
        }

        if (deviceSettings.ContainsKey("query"))
        {
          YamlMappingNode queries = (YamlMappingNode)deviceSettings["query"];

          Dictionary<string, object> queryMatch = new Dictionary<string, object>();
          if (settings.ContainsKey("query_match"))
          {
            queryMatch = (Dictionary<string, object>)settings["query_match"];
          }

          //var strings = currentQuery.Split('&').Select(kvp => string.Format("{0}={1}", kvp.Value, queryMatch[kvp.Key.ToString()]));
          //string path = string.Join("&", strings);
          //url = url + "?" + path;          
        }
      }

      return url;
    }

    private static Device getDevice(string currentUrl)
    {
      string device = "pc";

      Regex regex = new Regex(@"/(m|sp|i)/.*?$");
      Match m = regex.Match(currentUrl);
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
