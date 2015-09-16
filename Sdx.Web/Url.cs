using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Sdx.Web
{
  public class Url
  {
    private Dictionary<string, string> paramData;
    private String scheme;
    private String localPath;
    private String domain;

    //コンストラクタ
    public Url(string urlStr)
    {
      //@var System.Uri
      var uri = new Uri(urlStr);
      this.paramData = new Dictionary<string, string>();

      //パス用の情報を保存
      this.Scheme = uri.Scheme;
      this.Domain = uri.Host;
      this.LocalPath = uri.LocalPath;

      //各パラメータを連想配列でしまっておく
      string[] paramList = uri.Query.Trim('?').Split('&');
      paramList.ToList().ForEach(str => {
        string[] tmp = str.Split('=');
        this.paramData[tmp[0]] = tmp[1];
      });
    }

    private string BuildQueryString(Dictionary<string,string> param)
    {
      if (param.Count == 0)
      {
        return "";
      }

      var sb = new StringBuilder();
      sb.Append("?");
      param.ToList().ForEach(
        kv => sb.AppendFormat("{0}={1}&", kv.Key, kv.Value)
      );

      return sb.ToString().TrimEnd('&');
    }

    private string BuildPath()
    {
      string path = string.Format(
        "{0}://{1}{2}", this.Scheme, this.Domain, this.LocalPath
      );
      return path;
    }

    public string Build()
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(this.paramData);
      return path + query;
    }

    public string Build(Dictionary<string, string> add)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.paramData
          .Where(p => add.ContainsKey(p.Key) == false)
          .Concat(add)
          .ToDictionary(p => p.Key, p => p.Value)
      );
      return path + query;
    }

    public string Build(List<string> exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.paramData
          .Where(p => exclude.Contains(p.Key) == false)
          .ToDictionary(p => p.Key, p => p.Value)
      );
      return path + query;
    }

    public string Build(string[] exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.paramData
          .Where(p => exclude.Contains(p.Key) == false)
          .ToDictionary(p => p.Key, p => p.Value)
      );
      return path + query;
    }

    public string Domain
    {
      get
      {
        return this.domain;
      }

      set
      {
        this.domain = value;
      }
    }

    public string LocalPath
    {
      get
      {
        return this.localPath;
      }

      set
      {
        this.localPath = value;
      }
    }

    public Dictionary<string, string> Param
    {
      get
      {
        return this.paramData;
      }

      set
      {
        this.paramData = value;
      }
    }

    public string Scheme
    {
      get
      {
        return this.scheme;
      }

      set
      {
        this.scheme = value;
      }
    }
  }
}
