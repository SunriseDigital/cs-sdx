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

    private YamlMappingNode currentDeviceQuery = new YamlMappingNode();

    private YamlSequenceNode yamlSettings = new YamlSequenceNode();

    public string currentUrl { get; set; }

    private string targetDevice { get; set; }

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

    public DeviceTable(Device device, string url, string path)
    {
      targetDevice = DeviceString(device);
      currentUrl = url;

      using (FileStream fs = new FileStream(path, FileMode.Open))
      {
        using (var input = new StreamReader(fs, Encoding.GetEncoding("utf-8")))
        {
          var yaml = new YamlStream();
          yaml.Load(input);
          var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

          yamlSettings = (YamlSequenceNode)mapping.Children[new YamlScalarNode("page")];
        }
      }
    }

    public void loadSettings()
    {
      foreach (YamlMappingNode pageYaml in yamlSettings)
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
    }

    public static Sdx.Web.DeviceTable Current { get; set; }

    public bool IsMatch(Device device)
    {
      if (!settings.ContainsKey(DeviceString(device)))
      {
        return false;
      }
      
      Dictionary<string, object> currentDeviceSettings = (Dictionary<string, object>)settings[DeviceString(device)];

      string[] splitUrl = currentUrl.Split('?');
      string[] currentPaths = splitUrl[0].Split('/');

      string[] settingPaths = currentDeviceSettings["url"].ToString().Split('/');

      if (currentPaths.Length != settingPaths.Length)
      {
        return false;
      }

      var pathNotMatch =
        currentPaths
          .Select((item, index) => new { Index = index, Value = item })
          .Any(item => !PathCheck(item.Value, settingPaths[item.Index]));

      if (pathNotMatch)
      {
        return false;
      }

      if (currentDeviceSettings.ContainsKey("query"))
      {
        //対応表にqueryがあるのに現在のURLにクエリがない
        if (splitUrl.Length <= 1)
        {
          return false;
        }

        Dictionary<string, string> currentQuery = splitUrl[1].Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);

        currentDeviceQuery = (YamlMappingNode)currentDeviceSettings["query"];
        if (!QueryCheck(currentQuery, currentDeviceQuery))
        {
          return false;
        }
      }

      return true;
    }

    private bool PathCheck(string currentPath, string settingPath)
    {
      if (currentPath != settingPath)
      {
        //不要な正規表現チェックを避ける
        if(settingPath.IndexOf("{") == 0)
        {
          Regex reg = new Regex(@"^{([a-zA-Z0-9]+):(.*)}$", RegexOptions.Compiled);
          Match match = reg.Match(settingPath);
          if (match.Success)
          {
            //URL組み立て時の置換用の変数確保
            if (!replaceWords.ContainsKey(match.Result("$1").ToString()))
            {
              Regex r = new Regex(match.Result("$2").ToString(), RegexOptions.Compiled);
              Match m = r.Match(currentPath);
              if (!m.Success)
              {
                return false;
              }
              replaceWords.Add(match.Result("$1").ToString(), currentPath);
            }
            return true;
          }
        }

        return false;
      }

      return true;
    }

    private Dictionary<string, object> ReplaceQueryMatchKey(Dictionary<string, object> queryMatch, YamlMappingNode queries)
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

    private bool QueryCheck(Dictionary<string, string> currentQuery, YamlMappingNode deviceQuerySettings)
    {
      //先にquery_matchの処理
      if (settings.ContainsKey("query_match"))
      {
        var queryMatch = (Dictionary<string, object>)settings["query_match"];
        queryMatch = ReplaceQueryMatchKey(queryMatch, deviceQuerySettings);
        
        var queryNotMatchCheck =
          currentQuery
            .Where(w => queryMatch.ContainsKey(w.Key))
            .Where(w => queryMatch[w.Key].ToString() != "")
            .Any(q => queryMatch[q.Key].ToString() != q.Value);

        if (queryNotMatchCheck)
        {
          return false;
        }
      }

      //対応表に書かれたクエリが現在のURLにない
      if (deviceQuerySettings.Children.Any(q => !currentQuery.ContainsKey(q.Value.ToString())))
      {
        return false;
      }

      return true;
    }

    public string GetUrl(Device device)
    {
      loadSettings();
      IsMatch(device);

      string url = "";
      if (settings.ContainsKey(DeviceString(device)))
      {
        Dictionary<string, object> targetDeviceSettings = (Dictionary<string, object>)settings[DeviceString(device)];
        url = targetDeviceSettings["url"].ToString();

        if (replaceWords.Count > 0)
        {
          foreach(var word in replaceWords)
          {
            string pattern = @"{([a-zA-Z0-9]+):(.*)}";
            Regex reg = new Regex(pattern, RegexOptions.Compiled);
            Match match = reg.Match(url);
            if (match.Success)
            {
              url = Regex.Replace(url, pattern, replaceWords[match.Result("$1").ToString()]);
            }
          }
        }

        string query = "";
        string[] currentRawUrl = HttpContext.Current.Request.RawUrl.Split('?');
        if (currentRawUrl.Length > 1)
        {
          query = currentRawUrl[1];
        }

        if (targetDeviceSettings.ContainsKey("query"))
        {
          var rawQuery = query.Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);
          YamlMappingNode targetDeviceQuery = (YamlMappingNode)targetDeviceSettings["query"];

          foreach (var matchQuery in currentDeviceQuery)
          {
            var searchKey = targetDeviceQuery.Children[new YamlScalarNode(matchQuery.Key.ToString())].ToString();
            if (targetDeviceQuery.Children.ContainsKey(matchQuery.Key) && !rawQuery.ContainsKey(searchKey))
            {
              //keyの置き替え
              rawQuery.Add(searchKey, rawQuery[matchQuery.Value.ToString()]);
              rawQuery.Remove(matchQuery.Value.ToString());
            }
          }

          List<string> tmp = new List<string>();
          rawQuery.ToList().ForEach(q => tmp.Add(q.Key + "=" + q.Value));
          query = String.Join("&", tmp);
        }

        if (!string.IsNullOrEmpty(query))
        {
          url = url + "?" + query;
        }
      }

      return url;
    }

    private static Device GetDevice(string currentUrl)
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

    private static string DeviceString(Device device)
    {
      string[] strings = { "pc", "sp", "mb"};
      return strings[(int)device];
    }
  }
}
