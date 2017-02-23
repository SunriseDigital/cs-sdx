using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdx.Web
{
  public class Url : ICloneable, IUrl
  {
    //コンストラクタ
    public Url(string urlStr)
    {
      Uri uri;
      if(urlStr.StartsWith("http"))
      {
        uri = new Uri(urlStr);
        this.Scheme = uri.Scheme;
        this.Domain = uri.Host;
      }
      else if(urlStr.StartsWith("//"))
      {
        uri = new Uri("http:" + urlStr);
        this.Domain = uri.Host;
      }
      else
      {
        uri = new Uri("http://sdx.com" + urlStr);
      }
      
      this.ParamList = new List<Tuple<string,string>>();
      this.ParamCount = new Dictionary<string, int>();

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
      if(this.Scheme != null && this.Domain != null)
      {
        return string.Format(
          "{0}://{1}{2}", Uri.EscapeUriString(this.Scheme), Uri.EscapeUriString(this.Domain), Uri.EscapeUriString(this.LocalPath)
        );
      }
      else
      {
        return Uri.EscapeUriString(this.LocalPath);
      }
    }

    public string Build(Dictionary<string, string> add)
    {
      if(add.Count == 0)
      {
        return this.Build();
      }

      var tpList = add.Select(kv => Tuple.Create(kv.Key, kv.Value)).ToList();
      return Build(tpList);
    }

    public string Build(List<Tuple<string, string>> add)
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(
        this.ParamList
          .Concat(add)
          .ToList()
      );
      return path + query;
    }

    public string Query
    {
      get
      {
        return BuildQueryString(ParamList);
      }
    }

    public string PathAndQuery
    {
      get
      {
        return LocalPath + Query;
      }
    }

    public string Build(NameValueCollection add)
    {
      if (add.Count == 0)
      {
        return this.Build();
      }

      var tpList = new List<Tuple<string, string>>();
      foreach (string key in add)
      {
        var values = add.GetValues(key);
        foreach (var value in values)
        {
          tpList.Add(Tuple.Create(key, value));
        }
      }

      return Build(tpList);
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

    private string localPath;
    public string LocalPath
    {
      get
      {
        return Uri.UnescapeDataString(this.localPath);
      }
      set
      {
        this.localPath = Uri.EscapeUriString(value);
      }
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

    public Url SetParam(string key, int value)
    {
      return this.SetParam(key, value.ToString());
    }

    public Url SetParam(string key, string value)
    {
      if(!HasParam(key))
      {
        AddParam(key, value);
        return this;
      }

      var found = false;
      ParamList = ParamList.Where(tp =>
      {
        if(tp.Item1 == key)
        {
          if (found)
          {
            return false;
          }

          found = true;
          return true;
        }

        return true;
      }).Select(tp => tp.Item1 == key ? Tuple.Create(key, value) : tp).ToList();
      ParamCount[Uri.EscapeUriString(key)] = 1;
      return this;
    }

    public Url AddParam(string key, int value)
    {
      return this.AddParam(key, value.ToString());
    }

    public Url AddParam(string key, string value)
    {
      //value が null の場合、直後の EscapeUriString() でコケるので空文字を入れておく
      value = value ?? "";
      this.ParamList.Add(Tuple.Create(Uri.EscapeUriString(key), Uri.EscapeUriString(value)));
      this.AddParamCount(Uri.EscapeUriString(key));
      return this;
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
      return this.ParamCount.Any(kv => kv.Key == Uri.EscapeUriString(key));
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

    public object Clone()
    {
      var cloned = (Url)this.MemberwiseClone();

      cloned.ParamList = new List<Tuple<string, string>>(this.ParamList);
      cloned.ParamCount = new Dictionary<string, int>(this.ParamCount);

      return cloned;
    }

    public string BuildExcept(params string[] exclude)
    {
      if (exclude.Count() == 0)
      {
        return Build();
      }
      return Build(exclude);
    }

    public string BuildWith(params string[] add)
    {
      if(add.Length % 2 != 0)
      {
        throw new ArgumentException("Illegal key value pair count.");
      }

      if(add.Count() == 0)
      {
        return Build();
      }

      var tpList = new List<Tuple<string, string>>();
      for (int i = 0; i < add.Length; i+=2)
      {
        tpList.Add(Tuple.Create(add[i], add[i + 1]));
      }

      return Build(tpList);
    }

    public void ReplaceParamKey(string from, string to)
    {
      for (int i = 0; i < ParamList.Count; i++)
      {
        if(ParamList[i].Item1 == from)
        {
          ParamList[i] = Tuple.Create(to, ParamList[i].Item2);
        }
      }
    }



    #region IUrlのメソッド
    public string Build()
    {
      string path = this.BuildPath();
      string query = this.BuildQueryString(this.ParamList);
      return path + query;
    }

    public Url ToUrl()
    {
      return this;
    }

    public IUrl Next(string key, int defaultValue = 1)
    {
      return new Manipurator(this).Next(key, defaultValue);
    }

    public IUrl Prev(string key)
    {
      return new Manipurator(this).Prev(key);
    }

    public IUrl Add(string key, string value)
    {
      return new Manipurator(this).Add(key, value);
    }

    public IUrl Remove(string key)
    {
      return new Manipurator(this).Remove(key);
    }

    public IUrl Set(string key, string value)
    {
      return new Manipurator(this).Set(key, value);
    }
    #endregion

    #region Manipuratorの定義
    private class Manipurator : IUrl
    {
      private Url url;
      internal Manipurator(Url url)
      {
        this.url = (Url)url.Clone();
      }

      public Url ToUrl()
      {
        return url;
      }

      public string Build()
      {
        return ToUrl().Build();
      }

      /// <summary>
      /// 指定したパラメータの数字を一つ進めたURLを生成する。ページネーションのあるページで次のページのURLを取得する場合に利用可能。
      /// </summary>
      /// <param name="pageParamKey"></param>
      /// <returns></returns>
      public IUrl Next(string key, int defaultValue = 1)
      {
        var value = defaultValue;
        if (url.HasParam(key))
        {
          value = Int32.Parse(url.GetParam(key));
        }

        url.SetParam(key, value + 1);

        return this;
      }

      public IUrl Prev(string key)
      {
        if (!url.HasParam(key))
        {
          throw new Exception("Missing " + key + " column");
        }

        var value = Int32.Parse(url.GetParam(key));
        url.SetParam(key, value - 1);

        return this;
      }

      public IUrl Add(string key, string value)
      {
        url.AddParam(key, value);
        return this;
      }

      public IUrl Remove(string key)
      {
        url.RemoveParam(key);
        return this;
      }

      public IUrl Set(string key, string value)
      {
        url.SetParam(key, value);
        return this;
      }
    }
    #endregion
  }
}