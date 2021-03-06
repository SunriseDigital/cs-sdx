﻿using System;
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

    private OsInfo os;
    public OsInfo Os
    {
      get
      {
        if (os == null)
        {
          os = new OsInfo(this);
        }

        return os;
      }
    }

    private DeviceInfo device;
    public DeviceInfo Device
    {
      get
      {
        if (device == null)
        {
          device = new DeviceInfo(this);
        }

        return device;
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

      public bool IsIe
      {
        get
        {
          if (!cache.ContainsKey("Props.IsIe"))
          {
            cache["Props.IsIe"] = Regex.IsMatch(userAgent.Value, "(msie|MSIE|(T|t)rident)", RegexOptions.Compiled);
          }

          return (bool)cache["Props.IsIe"];
        }
      }
    }

    /// <summary>
    /// OS情報。OSプロパティーからアクセス。
    /// </summary>
    public class OsInfo
    {
      private Dictionary<string, object> cache;
      private UserAgent userAgent;
      public OsInfo(UserAgent userAgent)
      {
        this.userAgent = userAgent;
        cache = new Dictionary<string, object>();
      }
    }

    /// <summary>
    /// Device情報。Deviceプロパティーからアクセス。
    /// </summary>
    public class DeviceInfo
    {
      private Dictionary<string, object> cache;
      private UserAgent userAgent;
      public DeviceInfo(UserAgent userAgent)
      {
        this.userAgent = userAgent;
        cache = new Dictionary<string, object>();
      }

      public bool IsPc
      {
        get
        {
          return !IsMb && !IsSp;
        }
      }

      public bool IsMb
      {
        get
        {
          if (!cache.ContainsKey("Props.IsSp"))
          {
            cache["Props.IsSp"] = Regex.IsMatch(userAgent.Value, @"DoCoMo|UP.Browser|SoftBank|WILLCOM", RegexOptions.Compiled);
          }

          return (bool)cache["Props.IsSp"];
        }
      }

      public bool IsSp
      {
        get
        {
          if (!cache.ContainsKey("Props.IsMb"))
          {
            cache["Props.IsMb"] = Regex.IsMatch(userAgent.Value, @"iPhone|Android.*Mobile|Windows.*Phone", RegexOptions.Compiled);
          }

          return (bool)cache["Props.IsMb"];
        }
      }
    }
  }
}
