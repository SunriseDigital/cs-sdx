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

    private YamlMappingNode settingQuery = new YamlMappingNode();

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

            Device device = GetDevice(HttpContext.Current.Request.RawUrl);
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
      if (!settings.ContainsKey(DeviceString(device)))
      {
        return false;
      }
      
      Dictionary<string, object> deviceSettings = (Dictionary<string, object>)settings[DeviceString(device)];

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
          .Any(item => !PathCheck(item.Value, settingPaths[item.Index]));
      
      if (notEqualPaths)
      {
        return false;
      }

      //query_matchがあったときだけ対応表のqueryを見にいく
      if (settings.ContainsKey("query_match"))
      {
        if (splitUrl.Length <= 1)
        {
          return false;
        }

        Dictionary<string, string> currentQuery = splitUrl[1].Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);

        if(!QueryCheck(currentQuery, deviceSettings)){
          return false;
        }        
      }

      if (deviceSettings.ContainsKey("query"))
      {
        //ここまで来たら対応表にmatchしたのでそのデバイスのクエリを持っておく
        settingQuery = (YamlMappingNode)deviceSettings["query"];
      }

      return true;
    }

    private bool PathCheck(string currentPath, string settingPath)
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

    private bool QueryCheck(Dictionary<string, string> currentQuery, Dictionary<string, object> deviceSettings)
    {
      Dictionary<string, object> queryMatch = (Dictionary<string, object>)settings["query_match"];
      YamlMappingNode queries = new YamlMappingNode();

      if (deviceSettings.ContainsKey("query"))
      {
        queries = (YamlMappingNode)deviceSettings["query"];        
        queryMatch = ReplaceQueryMatchKey(queryMatch, queries);
      }      

      //先にquery_matchの処理
      var queryNotMatchCheck = 
        currentQuery
          .Where(w => queryMatch.ContainsKey(w.Key))
          .Where(w => queryMatch[w.Key].ToString() != "")
          .Any(q => queryMatch[q.Key].ToString() != q.Value);

      if (queryNotMatchCheck)
      {
        return false;
      }

      if (queries.Children.Any(s => !currentQuery.ContainsKey(s.Value.ToString())))
      {
        return false;
      }
   
      return true;
    }

    public string GetUrl(Device device)
    {
      string url = "";
      string query = "";

      if (settings.ContainsKey(DeviceString(device)))
      {
        Dictionary<string, object> deviceSettings = (Dictionary<string, object>)settings[DeviceString(device)];
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

//ここテスト用。本来は実際のURLから。
//string testUrl = "http://www.furonavi.com/yoshiwara/shop/?tg_prices_high=1&button=on";
//string[] currentRawUrl = testUrl.Split('?');
        string[] currentRawUrl = HttpContext.Current.Request.RawUrl.Split('?');

        if (deviceSettings.ContainsKey("query"))
        {          
          var rawQuery = currentRawUrl[1].Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);
          YamlMappingNode deviceQuery = (YamlMappingNode)deviceSettings["query"];

          foreach (var q in settingQuery)
          {
            if (deviceQuery.Children.ContainsKey(q.Key) && !rawQuery.ContainsKey(deviceQuery.Children[new YamlScalarNode(q.Key.ToString())].ToString()))
            {
              //keyの置き替え
              rawQuery.Add(deviceQuery.Children[new YamlScalarNode(q.Key.ToString())].ToString(), rawQuery[q.Value.ToString()]);
              rawQuery.Remove(q.Value.ToString());
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
