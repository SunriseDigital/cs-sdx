﻿using System;
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
      this.paramData = new Dictionary<string, string>();

      //各パラメータを連想配列でしまっておく
      string[] paramList = this.uri.Query.Trim('?').Split('&');
      paramList.ToList().ForEach(str => {
        string[] tmp = str.Split('=');
        this.paramData[tmp[0]] = tmp[1];
      });
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
  }
}
