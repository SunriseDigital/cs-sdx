using System;
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

    private string BuildQueryString(Dictionary<string,string> param)
    {
      string query = "";
      foreach(KeyValuePair<string, string> pair in param)
      {
        query += string.Format("{0}={1}&", pair.Key, pair.Value);
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
      if(this.param_data.Count > 0)
      {
        string query = this.BuildQueryString(this.param_data);
        return path + "?" + query.Trim('&');
      }
      return path;
    }

    public string Build(Dictionary<string, string> param)
    {
      string path = this.BuildPath();
      string query = "";
      if (this.param_data.Count > 0)
      {
        query = "?" + this.BuildQueryString(this.param_data);
      }
      string add_query = this.BuildQueryString(param);
      return path + query + add_query.Trim('&');
    }
  }
}
