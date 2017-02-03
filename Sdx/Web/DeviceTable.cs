using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
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

    private Dictionary<string, YamlNode> currentPage = null;

    private YamlSequenceNode yamlSettings = new YamlSequenceNode();

    private string currentUrl;

    private Device targetDevice;

    private bool perfectCheck = false;

    private Dictionary<Device, string> matchUrls = new Dictionary<Device, string>();

    public MemoryCache MemoryCache { get; set; }

    public enum Device
    {
      Pc,
      Sp,
      Mb
    }

    public DeviceTable(Device device, string url, string path, MemoryCache memoryCache = null)
    {
      targetDevice = device;
      currentUrl = url;
      MemoryCache = memoryCache;

      if (MemoryCache == null || !MemoryCache.Contains(url))
      {
        if (!File.Exists(path))
        {
          throw new FileNotFoundException("Not Exists this FilePath");
        }

        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
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
    }

    private Dictionary<string, YamlNode> DetectPage()
    {
      var matchPage = new Dictionary<string, YamlNode>();

      foreach (YamlMappingNode pageYaml in yamlSettings)
      {
        var children = pageYaml.Children;
        YamlNode queryMatch = null;

        if (children.ContainsKey(new YamlScalarNode("query_match")) && children.ContainsKey(new YamlScalarNode("query_match_perfect")))
        {
          throw new InvalidOperationException("Only one can be specified query_match or query_match_perfect");
        }

        if (children.ContainsKey(new YamlScalarNode(DeviceString(targetDevice))))
        {
          if (children.ContainsKey(new YamlScalarNode("query_match")))
          {
            perfectCheck = false;
            queryMatch = (YamlMappingNode)children[new YamlScalarNode("query_match")];
          }

          if (children.ContainsKey(new YamlScalarNode("query_match_perfect")))
          {
            perfectCheck = true;

            queryMatch = children[new YamlScalarNode("query_match_perfect")];
          }

          if (IsMatch(targetDevice, children[new YamlScalarNode(DeviceString(targetDevice))], queryMatch))
          {
            //YamlNodeだとキーへのアクセスが冗長になるので、マッチしたブロックをDictionaryにして取り出しやすいようにする
            foreach (var item in pageYaml)
            {
              matchPage.Add(item.Key.ToString(), item.Value);
            }

            return matchPage;
          }
        }
      }

      return matchPage;
    }

    public static Sdx.Web.DeviceTable Current { get; set; }

    private bool IsMatch(Device device, YamlNode items, YamlNode queryMatch)
    {

      Dictionary<string, object> currentDeviceSettings = new Dictionary<string, object>();

      //YamlNodeだとキーへのアクセスが冗長になるので、Dictionaryして値を取り出しやすいようにする
      foreach (var item in (YamlMappingNode)items)
      {
        currentDeviceSettings.Add(item.Key.ToString(), item.Value);
      }

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

      if (queryMatch != null)
      {
        if (splitUrl.Length <= 1)
        {
          //query_matchの中身があるのに現在のURLにクエリがない
          if (queryMatch.GetType() == typeof(YamlMappingNode))
          {
            return false;
          }
        }
        else
        {
          //query_matchの中身が空なのに、現在のURLにクエリがある
          if (queryMatch.GetType() != typeof(YamlMappingNode))
          {
            return false;
          }

          Dictionary<string, string> currentQuery = splitUrl[1].Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);
          if (currentDeviceSettings.ContainsKey("query"))
          {
            return QueryCheck(currentQuery, queryMatch, (YamlMappingNode)currentDeviceSettings["query"]);
          }
          else
          {
            return QueryCheck(currentQuery, queryMatch);
          }
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

    private YamlNode ReplaceQueryMatchKey(YamlNode queryMatch, YamlMappingNode queries)
    {
      foreach (var query in queries)
      {
        var queryKey = new YamlScalarNode(query.Key.ToString());
        var children = ((YamlMappingNode)queryMatch).Children;
        if (children.ContainsKey(queryKey))
        {
          ((YamlMappingNode)queryMatch).Add(query.Value.ToString(), children[new YamlScalarNode(query.Key.ToString())]);
          children.Remove(new YamlScalarNode(query.Key.ToString()));
        }
      }

      return queryMatch;
    }

    private bool QueryCheck(Dictionary<string, string> currentQuery, YamlNode queryMatch, YamlMappingNode deviceQuerySettings)
    {
      //対応表に書かれたクエリが現在のURLにない
      if (deviceQuerySettings.Children.Any(q => !currentQuery.ContainsKey(q.Value.ToString())))
      {
        return false;
      }

      queryMatch = ReplaceQueryMatchKey(queryMatch, deviceQuerySettings);

      if (!QueryCheck(currentQuery, queryMatch))
      {
        return false;
      }

      return true;
    }

    private bool QueryCheck(Dictionary<string, string> currentQuery, YamlNode queryMatch)
    {
      var searchQuery = ((YamlMappingNode)queryMatch).Children;

      if(perfectCheck)
      {
        if (currentQuery.Any(w => !searchQuery.ContainsKey(new YamlScalarNode(w.Key))))
        {
          return false;
        }
      }

      if (searchQuery.Any(w => !currentQuery.ContainsKey(w.Key.ToString())))
      {
        return false;
      }

      var queryNotMatchCheck =
        currentQuery
          .Where(w => searchQuery.ContainsKey(new YamlScalarNode(w.Key)))
          .Where(w => searchQuery[new YamlScalarNode(w.Key)].ToString() != "")
          .Any(q => searchQuery[new YamlScalarNode(q.Key)].ToString() != q.Value);

      if (queryNotMatchCheck)
      {
        return false;
      }
      
      return true;
    }

    public string GetUrl(Device device)
    {
      if (MemoryCache != null && MemoryCache.Contains(currentUrl))
      {
        var settingCache = (Dictionary<Device, string>)MemoryCache[currentUrl];
        if (settingCache.ContainsKey(device))
        {
          return settingCache[device];
        }
      }

      if (matchUrls.ContainsKey(device))
      {
        return matchUrls[device];
      }

      string url = "";

      if (currentPage == null)
      {
        currentPage = DetectPage();
      }

      if (currentPage.ContainsKey(DeviceString(device)))
      {
        YamlMappingNode targetDeviceSettings = (YamlMappingNode)currentPage[DeviceString(device)];
        url = targetDeviceSettings.Children[new YamlScalarNode("url")].ToString();

        if (replaceWords.Count > 0)
        {
          string[] split = url.Split('/');
          foreach (var word in replaceWords)
          {
            url = url.Split('/')
              .Select(val => val.IndexOf("{" + word.Key + ":") >= 0 ? word.Value : val)
              .Aggregate((current, next) => current + "/" + next);
          }
        }

        string query = "";
        string[] currentRawUrl = currentUrl.Split('?');

        if (currentRawUrl.Length > 1)
        {
          query = currentRawUrl[1];
          var rawQuery = query.Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);

          if (targetDeviceSettings.Children.ContainsKey(new YamlScalarNode("query")))
          {
            YamlMappingNode targetDeviceQuery = (YamlMappingNode)targetDeviceSettings.Children[new YamlScalarNode("query")];
            YamlMappingNode currentPageQuery = (YamlMappingNode)((YamlMappingNode)currentPage[DeviceString(targetDevice)]).Children[new YamlScalarNode("query")];

            foreach (var matchQuery in currentPageQuery)
            {
              var searchKey = targetDeviceQuery.Children[new YamlScalarNode(matchQuery.Key.ToString())].ToString();
              if (targetDeviceQuery.Children.ContainsKey(matchQuery.Key) && !rawQuery.ContainsKey(searchKey))
              {
                //keyの置き替え
                rawQuery.Add(searchKey, rawQuery[matchQuery.Value.ToString()]);
                rawQuery.Remove(matchQuery.Value.ToString());
              }
            }
          }

          List<string> exclude_queries = new List<string>();
          if (currentPage.ContainsKey("exclude_build_query"))
          {
            foreach (var exclude_build_query in (YamlSequenceNode)currentPage["exclude_build_query"])
            {
              exclude_queries.Add(exclude_build_query.ToString());
            }
          }

          if (targetDeviceSettings.Children.ContainsKey(new YamlScalarNode("exclude_build_query")))
          {
            foreach (var exclude_build_query in (YamlSequenceNode)targetDeviceSettings.Children[new YamlScalarNode("exclude_build_query")])
            {
              exclude_queries.Add(exclude_build_query.ToString());
            }
          }

          if (exclude_queries != null)
          {
            foreach (var exclude in exclude_queries)
            {
              rawQuery.Remove(exclude);
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

        matchUrls[device] = url;

        if(MemoryCache != null)
        {
          Dictionary<Device, string> cacheUrlSetting = new Dictionary<Device, string>();
          if (MemoryCache.Contains(currentUrl))
          {
            cacheUrlSetting = (Dictionary<Device, string>)MemoryCache[currentUrl];
          }

          cacheUrlSetting.Add(device, url);
          MemoryCache.Set(currentUrl, cacheUrlSetting, new CacheItemPolicy());
        }
      }

      return url;
    }

    private static string DeviceString(Device device)
    {
      string[] strings = { "pc", "sp", "mb"};
      return strings[(int)device];
    }
  }
}
