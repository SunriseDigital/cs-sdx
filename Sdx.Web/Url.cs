using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class Url
  {
    private Uri _uri;
    private Dictionary<string, string> _query_data;

    //コンストラクタ
    public Url(string url_str)
    {
      //@System.Uri
      this._uri = new Uri(url_str);

      //クエリを分解して連想配列にする
      string[] param_list = this._uri.Query.Split('&');
      Dictionary<string, string> data = new Dictionary<string, string>();
      foreach (var item in param_list)
      {
        string[] item_part = item.Split('=');
        data.Add(item_part[0], item_part[1]);
      }
      this._query_data = data;
    }
  }
}
