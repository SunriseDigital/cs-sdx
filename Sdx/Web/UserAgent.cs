using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Sdx.Web
{
  /// <summary>
  /// ユーザーエージェントパーサーです。最初に全て実装するのは大変なので必要に応じてメソッドを追加してください。
  /// 実装は遅延初期化で、cacheにキャッシュしてください。
  /// </summary>
  public class UserAgent
  {
    public UserAgent(string userAgent)
    {
      Value = userAgent;
    }

    public string Value { get; private set; }

    private BrowserInfo browser;
    public BrowserInfo Browser
    {
      get
      {
        if(browser == null)
        {
          browser = new BrowserInfo(this);
        }

        return browser;
      }
    }

    private OSInfo os;
    public OSInfo OS
    {
      get
      {
        if (os == null)
        {
          os = new OSInfo(this);
        }

        return os;
      }
    }

    /// <summary>
    /// ブラウザー情報。Browserプロパティーからアクセス。
    /// </summary>
    public class BrowserInfo
    {
      private Dictionary<string, object> cache;
      private UserAgent userAgent;
      public BrowserInfo(UserAgent userAgent)
      {
        this.userAgent = userAgent;
        cache = new Dictionary<string, object>();
      }

      public bool IsIE
      {
        get
        {
          if (!cache.ContainsKey("Props.IsIE"))
          {
            cache["Props.IsIE"] = Regex.IsMatch(userAgent.Value, "(msie|MSIE|(T|t)rident)");
          }

          return (bool)cache["Props.IsIE"];
        }
      }
    }

    /// <summary>
    /// OS情報。OSプロパティーからアクセス。
    /// </summary>
    public class OSInfo
    {
      private Dictionary<string, object> cache;
      private UserAgent userAgent;
      public OSInfo(UserAgent userAgent)
      {
        this.userAgent = userAgent;
        cache = new Dictionary<string, object>();
      }
    }
  }
}
