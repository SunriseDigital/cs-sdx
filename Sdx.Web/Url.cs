using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Url
  {
    private Uri uri;
    private Dictionary<string, string> paramData;

    //コンストラクタ
    public Url(string urlStr)
    {
      //@var System.Uri
      this.uri = new Uri(urlStr);

      //クエリを分解して連想配列にする
      string[] paramList = this.uri.Query.Trim('?').Split('&');
      Dictionary<string, string> data = new Dictionary<string, string>();
      foreach (var item in paramList)
      {
        string[] tmp = item.Split('=');
        data[tmp[0]] = tmp[1];
      }
      this.paramData = data;
    }

    public string GetDomain()
    {
      return this.uri.Host;
    }

    public string GetParam(string key)
    {
      return this.paramData[key];
    }

    public string GetPath()
    {
      return this.uri.LocalPath;
    }

    public string GetProtocol()
    {
      return this.uri.Scheme;
    }

    public void SetParam(string key, string value)
    {
      this.paramData[key] = value;
    }

    private string BuildQueryString(Dictionary<string,string> param)
    {
      //ここに格納された値を最終的に "&" で連結してクエリ文字列にします
      var baseParams = new List<string>();
      foreach (KeyValuePair<string, string> pair in param)
      {
        var tmp = string.Format("{0}={1}", pair.Key, pair.Value);
        baseParams.Add(tmp);
      }
      var query = "";
      if(baseParams.Count > 0)
      {
        query = "?" + string.Join("&", baseParams);
      }
      return query;
    }

    private string BuildPath()
    {
      string path = string.Format(
        "{0}://{1}{2}", this.GetProtocol(), this.GetDomain(), this.GetPath()
      );
      return path;
    }

    public string Build()
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(this.paramData);
      return path + query;
    }

    public string Build(Dictionary<string, string> param)
    {
      string path = this.BuildPath();
      var list = new Dictionary<string, string>();

      foreach (KeyValuePair<string, string> item in this.paramData)
      {
        list[item.Key] = item.Value;
      }

      foreach (KeyValuePair<string, string> item in param)
      {
        list[item.Key] = item.Value;
      }

      string query = this.BuildQueryString(list);
      return path + query;
    }

    public string Build(List<string> excludeList)
    {
      string path = this.BuildPath();
      var excludedDic = this.paramData
        .Where(p => excludeList.Contains(p.Key) == false)
        .ToDictionary(p => p.Key, p => p.Value)
      ;
      string query = this.BuildQueryString(excludedDic);
      return path + query;
    }
  }
}
