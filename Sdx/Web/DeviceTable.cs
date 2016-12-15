﻿using System;
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

    private Dictionary<string, YamlNode> currentPage = null;

    private YamlSequenceNode yamlSettings = new YamlSequenceNode();

    private string currentUrl;

    private Device targetDevice;

    private Dictionary<Device, string> matchUrls = new Dictionary<Device, string>();

    public enum Device
    {
      Pc,
      Sp,
      Mb
    }

    public DeviceTable(Device device, string url, string path)
    {
      targetDevice = device;
      currentUrl = url;

      if (!File.Exists(path))
      {
        throw new InvalidOperationException("Not Exists this FilePath");
      }

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

    private Dictionary<string, YamlNode> DetectPage()
    {
      var matchPage = new Dictionary<string, YamlNode>();      

      foreach (YamlMappingNode pageYaml in yamlSettings)
      {
        var children = pageYaml.Children;
        YamlMappingNode queryMatch = null;

        if (children.ContainsKey(new YamlScalarNode(DeviceString(targetDevice))))
        {
          if (children.ContainsKey(new YamlScalarNode("query_match")))
          {
            queryMatch = (YamlMappingNode)children[new YamlScalarNode("query_match")];
          }

          if (IsMatch(targetDevice, children[new YamlScalarNode(DeviceString(targetDevice))], queryMatch))
          {
            foreach (var item in pageYaml)
            {
              matchPage.Add(item.Key.ToString(), (YamlMappingNode)item.Value);
            }
          }
        }
      }

      return matchPage;
    }

    public static Sdx.Web.DeviceTable Current { get; set; }

    private bool IsMatch(Device device, YamlNode items, YamlMappingNode queryMatch)
    {
      Dictionary<string, object> currentDeviceSettings = new Dictionary<string, object>();

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

      if (currentDeviceSettings.ContainsKey("query"))
      {
        //対応表にqueryがあるのに現在のURLにクエリがない
        if (splitUrl.Length <= 1)
        {
          return false;
        }

        Dictionary<string, string> currentQuery = splitUrl[1].Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);

        if (!QueryCheck(currentQuery, (YamlMappingNode)currentDeviceSettings["query"], queryMatch))
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

    private YamlMappingNode ReplaceQueryMatchKey(YamlMappingNode queryMatch, YamlMappingNode queries)
    {
      foreach (var query in queries)
      {
        var queryKey = new YamlScalarNode(query.Key.ToString());
        if (queryMatch.Children.ContainsKey(queryKey))
        {
          queryMatch.Add(query.Value.ToString(), queryMatch.Children[new YamlScalarNode(query.Key.ToString())]);
          queryMatch.Children.Remove(new YamlScalarNode(query.Key.ToString()));
        }
      }

      return queryMatch;
    }

    private bool QueryCheck(Dictionary<string, string> currentQuery, YamlMappingNode deviceQuerySettings, YamlMappingNode queryMatch)
    {
      //先にquery_matchの処理
      if (queryMatch != null)
      {
        queryMatch = ReplaceQueryMatchKey((YamlMappingNode)queryMatch, deviceQuerySettings);

        var searchQuery = queryMatch.Children;
        
        var queryNotMatchCheck =
          currentQuery
            .Where(w => searchQuery.ContainsKey(new YamlScalarNode(w.Key)))
            .Where(w => searchQuery[new YamlScalarNode(w.Key)].ToString() != "")
            .Any(q => searchQuery[new YamlScalarNode(q.Key)].ToString() != q.Value);

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
        }

        if (targetDeviceSettings.Children.ContainsKey(new YamlScalarNode("query")))
        {
          var rawQuery = query.Split('&').Select(s => s.Split('=')).ToDictionary(n => n[0], n => n[1]);
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

          List<string> tmp = new List<string>();
          rawQuery.ToList().ForEach(q => tmp.Add(q.Key + "=" + q.Value));
          query = String.Join("&", tmp);
        }

        if (!string.IsNullOrEmpty(query))
        {
          url = url + "?" + query;
        }

        matchUrls[device] = url;
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
