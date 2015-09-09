﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Url
  {
    private Uri uri;
    private Dictionary<string, string> param_data;

    //コンストラクタ
    public Url(string url_str)
    {
      //@var System.Uri
      this.uri = new Uri(url_str);

      //クエリを分解して連想配列にする
      string[] param_list = this.uri.Query.Trim('?').Split('&');
      Dictionary<string, string> data = new Dictionary<string, string>();
      foreach (var item in param_list)
      {
        string[] item_part = item.Split('=');
        data[item_part[0]] = item_part[1];
      }
      this.param_data = data;
    }

    public string GetDomain()
    {
      return this.uri.Host;
    }

    public string GetParam(string key_name)
    {
      return this.param_data[key_name];
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
      this.param_data[key] = value;
    }

    private string BuildQueryString(object param = null)
    {
      //ここに格納された値を最終的に "&" で連結してクエリ文字列にします
      var baseParams = new List<string>();
      foreach (KeyValuePair<string, string> item in this.param_data)
      {
        var tmp = string.Format("{0}={1}", item.Key, item.Value);
        baseParams.Add(tmp);
      }

      if(param is Dictionary<string, string>)
      {
        //コンパイル時には型が確定していないためキャストしています
        var dicParams = (Dictionary<string, string>)param;
        foreach (KeyValuePair<string, string> pair in dicParams)
        {
          var tmp = string.Format("{0}={1}", pair.Key, pair.Value);
          baseParams.Add(tmp);
        }
      }

      if(param is List<string>)
      {
        //コンパイル時には型が確定していないためキャストしています
        var excludeList = (List<string>)param;
        var excludedData = this.param_data
          .Where(p => excludeList.Contains(p.Key) == false)
          .ToList()//この時点で List<KeyValuePair<string,string>> 型のListになる
        ;
        var tmplist = new List<string>();
        foreach (KeyValuePair<string, string> data in excludedData)
        {
          tmplist.Add(string.Format("{0}={1}", data.Key, data.Value));
        }
        baseParams = tmplist;
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
      string query = this.BuildQueryString();
      return path + query;
    }

    public string Build(Dictionary<string, string> param)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(param);
      return path + query;
    }

    public string Build(List<string> param)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(param);
      return path + query;
    }
  }
}
