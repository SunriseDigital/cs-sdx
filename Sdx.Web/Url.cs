using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Url
  {
    private Uri _uri;
    private Dictionary<string, string> _param_data;

    //コンストラクタ
    public Url(string url_str)
    {
      //@System.Uri
      this._uri = new Uri(url_str);

      //クエリを分解して連想配列にする
      string[] param_list = this._uri.Query.Trim('?').Split('&');
      Dictionary<string, string> data = new Dictionary<string, string>();
      foreach (var item in param_list)
      {
        string[] item_part = item.Split('=');
        data.Add(item_part[0], item_part[1]);
      }
      this._param_data = data;
    }

    public string GetDomain()
    {
      return this._uri.Host;
    }

    public string GetParam(string key_name)
    {
      return this._param_data[key_name];
    }

    public string GetPath()
    {
      return this._uri.LocalPath;
    }

    public string GetProtocol()
    {
      return this._uri.Scheme;
    }

    public void SetParam(string key, string value)
    {
      this._param_data.Add(key, value);
    }

    public string BuildQueryString()
    {
      string query = "?";
      foreach(KeyValuePair<string, string> pair in this._param_data)
      {
        query += string.Format("{0}={1}&", pair.Key, pair.Value);
      }
      return query.Trim('&');//末尾の'&'は不要なのでカットして返す
    }

    public string Build()
    {
      string path = string.Format(
        "{0}://{1}{2}", this.GetProtocol(), this.GetDomain(), this.GetPath()
      );
      string query = this.BuildQueryString();
      return path + query;
    }
  }
}
