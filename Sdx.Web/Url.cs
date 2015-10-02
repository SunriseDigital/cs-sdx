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
      this.RoopCount = new Dictionary<string, int>();

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
          this.AddRoopCount(tmp[0]);
        });
      }
    }

    private string BuildQueryString(List<KeyValuePair<string,string>> param)
    {
      if (param.Count == 0)
      {
        return "";
      }

      var sb = new StringBuilder();
      sb.Append("?");
      param.ForEach(kv => sb.AppendFormat("{0}={1}&", kv.Key, kv.Value));

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
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Concat(add.ToList())
          .ToList()
      );
      return path + query;
    }

    public string Build(List<string> exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Where(kv => exclude.Contains(kv.Key) == false)
          .ToList()
      );
      return path + query;
    }

    public string Build(string[] exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Where(kv => exclude.Contains(kv.Key) == false)
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
      this.RemoveParam(key);
      this.AddParam(key, value);
    }

    public void AddParam(string key, string value)
    {
      this.ParamList.Add(new KeyValuePair<string,string>(key, value));
      this.AddRoopCount(key);
    }

    public void RemoveParam(string key)
    {
      this.ParamList.RemoveAll(kv => kv.Key == key);
      this.RoopCount[key] = 0;
    }

    /// <summary>
    /// 指定された key で検索し "最初に一致した" 要素の値を返す
    /// </summary>
    public string GetParam(string key)
    {
      return this.GetParams(key).First();
    }

    public List<string> GetParams(string key)
    {
      var list = new List<string>();
      foreach(var kv in this.ParamList)
      {
        if (kv.Key == key)
        {
          list.Add(kv.Value);
          if (list.Count == this.RoopCount[key])
          {
            break;
          }
        }
      }

      //存在しないキーを指定された場合以外は、必ず何かしら list に入っているので
      //この段階で例外を投げる
      if (list.Count == 0)
      {
        throw new KeyNotFoundException();
      }
      return list;
    }

    /// <summary>
    /// ParamList の各キー毎のループ回数を管理するプロパティ
    /// 同じ名前のキーがセットされたらカウントをアップし、
    /// 削除されたらカウントを0に戻す
    /// </summary>
    private Dictionary<string, int> RoopCount
    {
      get; set;
    }

    private void AddRoopCount(string key)
    {
      if (this.RoopCount.ContainsKey(key))
      {
        this.RoopCount[key]++;
      }
      else
      {
        this.RoopCount[key] = 1;
      }
    }
  }
}
