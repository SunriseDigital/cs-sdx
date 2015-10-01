using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Url
  {
    //コンストラクタ
    public Url(string urlStr)
    {
      //@var System.Uri
      var uri = new Uri(urlStr);
      this.ParamList = new List<KeyValuePair<string,string>>();

      //パス用の情報を保存
      this.Scheme = uri.Scheme;
      this.Domain = uri.Host;
      this.LocalPath = uri.LocalPath;

      //クエリ文字列があればパラメータ毎に連想配列でしまっておく
      if (uri.Query.Contains('?'))
      {
        string query = uri.Query.Trim('?');
        var QueryStringList = new List<string>();

        //パラメータの項目数が複数か単独か
        if(query.Contains('&'))
        {
          QueryStringList = query.Split('&').ToList();
        }
        else
        {
          QueryStringList.Add(query);
        }

        QueryStringList.ForEach(str =>
        {
          string[] tmp = str.Split('=');
          this.ParamList.Add(new KeyValuePair<string, string>(tmp[0], tmp[1]));
        });
      }
    }

    private string BuildQueryString(List<Dictionary<string,string>> param)
    {
      if (param.Count == 0)
      {
        return "";
      }

      var sb = new StringBuilder();
      sb.Append("?");
      param.ForEach(dic => {
        if(dic.Count > 0) {
          sb.AppendFormat("{0}={1}&", dic.First().Key, dic.First().Value);
        }
      });

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
      string query = this.BuildQueryString(this.ParamList);
      return path + query;
    }

    public string Build(Dictionary<string, string> add)
    {
      var tmpList = new List<Dictionary<string, string>>() { add };
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Where(dic => add.ContainsKey(dic.First().Key) == false)
          .Concat(tmpList)
          .ToList()
      );
      return path + query;
    }

    public string Build(List<string> exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Where(dic => exclude.Contains(dic.First().Key) == false)
          .ToList()
      );
      return path + query;
    }

    public string Build(string[] exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Where(dic => exclude.Contains(dic.First().Key) == false)
          .ToList()
      );
      return path + query;
    }

    public string Domain
    {
      get; set;
    }

    public string LocalPath
    {
      get; set;
    }

    private List<KeyValuePair<string, string>> ParamList
    {
      get; set;
    }

    public string Scheme
    {
      get; set;
    }

    public void SetParam(string key, string value)
    {
      var tmp = new Dictionary<string,string>(){{key, value}};
      this.ParamList.Add(tmp);
    }

    public string GetParam(string key)
    {
      return this.GetParamList(key).Last();
    }

    public List<string> GetParamList(string key)
    {
      var list = new List<string>();
      this.ParamList.ForEach(dic =>
      {
        if (dic.ContainsKey(key))
        {
          list.Add(dic[key]);
        }
      });

      //存在しないキーを指定された場合以外は、必ず何かしら list に入っているので
      //この段階で例外を投げる
      if (list.Count == 0)
      {
        throw new KeyNotFoundException();
      }
      return list;
    }
  }
}
