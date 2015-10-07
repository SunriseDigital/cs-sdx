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
      this.ParamList = new List<Tuple<string,string>>();
      this.ParamCount = new Dictionary<string, int>();

      //System.Uri を通しただけでは Host, LocalPath は未エスケープなのでここでする
      this.Scheme = uri.Scheme;
      this.Domain = uri.Host;
      this.LocalPath = uri.LocalPath;

      //クエリーを分解して保存
      foreach (var str in uri.Query.Trim('?').Split('&'))
      {
        string[] tmp = str.Split('=');
        if (tmp.Length > 1)
        {
          this.ParamList.Add(Tuple.Create(tmp[0], tmp[1]));
          this.AddParamCount(tmp[0]);
        }
      }
    }

    private string BuildQueryString(List<Tuple<string,string>> param)
    {
      if (param.Count == 0)
      {
        return "";
      }

      var sb = new StringBuilder();
      sb.Append("?");
      param.ForEach(tp => sb.AppendFormat("{0}={1}&", tp.Item1, tp.Item2));

      return sb.ToString().TrimEnd('&');
    }

    private string BuildPath()
    {
      string path = string.Format(
        "{0}://{1}{2}", Uri.EscapeUriString(this.Scheme), Uri.EscapeUriString(this.Domain), Uri.EscapeUriString(this.LocalPath)
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
      if(add.Count == 0)
      {
        return this.Build();
      }

      var tpList = add.Select(kv => Tuple.Create(kv.Key, kv.Value)).ToList();
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Concat(tpList)
          .ToList()
      );
      return path + query;
    }

    public string Build(IEnumerable<string> exclude)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Where(tp => exclude.Contains(tp.Item1) == false)
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

    /// <summary>
    /// セットされたパラメータを管理するプロパティ
    /// Tuple<Item1 = パラメータのキー, Item2=パラメータの値> で
    /// セットされます
    /// </summary>
    private List<Tuple<string, string>> ParamList
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
      this.ParamList.Add(Tuple.Create(Uri.EscapeUriString(key), Uri.EscapeUriString(value)));
      this.AddParamCount(Uri.EscapeUriString(key));
    }

    public void RemoveParam(string key)
    {
      this.ParamList.RemoveAll(tp => tp.Item1 == Uri.EscapeUriString(key));
      this.ParamCount[Uri.EscapeUriString(key)] = 0;
    }

    /// <summary>
    /// 指定された key で検索し "最初に一致した" 要素の値を返す
    /// </summary>
    public string GetParam(string key)
    {
      var list = this.GetParams(key);
      if(list.Count == 0)
      {
        throw new KeyNotFoundException();
      }
      return list[0];
    }

    public List<string> GetParams(string key)
    {
      var escapedKey = Uri.EscapeUriString(key);
      var list = new List<string>();
      foreach(var tp in this.ParamList)
      {
        if (tp.Item1 == escapedKey)
        {
          list.Add(Uri.UnescapeDataString(tp.Item2));
          if (list.Count == this.ParamCount[escapedKey])
          {
            break;
          }
        }
      }
      return list;
    }

    public bool HasParam(string key)
    {
      return this.ParamCount.Any(kv => kv.Key == key);
    }

    /// <summary>
    /// ParamList の各キー毎の要素数を管理するプロパティ
    /// 同じ名前のキーがセットされたらカウントをアップし、
    /// 削除されたらカウントを0に戻す
    /// </summary>
    private Dictionary<string, int> ParamCount
    {
      get; set;
    }

    private void AddParamCount(string key)
    {
      if (this.ParamCount.ContainsKey(key))
      {
        this.ParamCount[key]++;
      }
      else
      {
        this.ParamCount[key] = 1;
      }
    }
  }
}
