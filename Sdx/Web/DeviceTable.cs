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
    private Dictionary<string, string> regex = new Dictionary<string, string>();

    private Dictionary<string, string> queries = new Dictionary<string, string>();

    private Dictionary<string, string> urls = new Dictionary<string, string>();

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
      string settingUrl = null;

      foreach (var item in (Dictionary<string, object>)settings[device.ToString()])
      {
        if (item.Key.ToString() == "url")
        {
           settingUrl = item.Value.ToString();
        }
        
        if (item.Key.ToString() == "query")
        {
          foreach (var query in (YamlMappingNode)item.Value)
          {
            if (!queries.ContainsKey(query.Key.ToString()))
            {
              queries.Add(query.Key.ToString(), query.Value.ToString());
            }
          }
        }
      }

      string[] splitUrl = url.Split('?');
      List<string> path = splitUrl[0].Split('/').ToList();

      List<string> settingPath = settingUrl.Split('/').ToList();

      if(queries.Count > 0 && splitUrl.Length <= 1){
        return false;
      }

      if(path.Count != settingPath.Count){
        return false;
      }

      for (int i = 0; i < path.Count; i++)
      {
        Regex reg = new Regex(@"^{([a-zA-Z0-9]+):(.*)}$");
        Match match = reg.Match(settingPath[i]);
        if (match.Success)
        {
          //置換用の変数確保
          if (!regex.ContainsKey(match.Result("$1").ToString()))
          {
            Regex r = new Regex(match.Result("$2").ToString());
            Match m = r.Match(path[i]);
            if (!m.Success)
            {
              return false;
            }
            regex.Add(match.Result("$1").ToString(), path[i]);
          }
        }
        else
        {
          if (path[i] != settingPath[i])
          {
            return false;
          }
        }
      }

      if (splitUrl.Length > 1 && !checkQuery(splitUrl[1].Split('&')))
      {
        return false;
      }

      return true;
    }

    private bool checkQuery(string[] splitQuery)
    {
      Dictionary<string, object> queryMatch = new Dictionary<string, object>();
      if (settings.ContainsKey("query_match"))
      {
        queryMatch = (Dictionary<string, object>)settings["query_match"];
      }

      if (queries.Count < 1)
      {
        return false;
      }
      Console.WriteLine(splitQuery[1]);
      Dictionary<string, string> dic = splitQuery.ToDictionary(n => n.Split('=')[0], n => n.Split('=')[1]);
      foreach (var query in dic)
      {
        if (queries.ContainsValue(query.Key))
        {
          var key = queries.First(x => x.Value == query.Key).Key;

          //query_matchがある場合値まで比較
          if (queryMatch.ContainsKey(key) && queryMatch[key].ToString() != query.Value)
          {
            return false;
          }
        }
        else
        {
          //queriesにkeyがないということはそもそもURLが違うのでfalse
          return false;
        }
      }
   
      return true;
    }

    public string GetUrl(Device device)
    {
      return "";
    }
  }
}
