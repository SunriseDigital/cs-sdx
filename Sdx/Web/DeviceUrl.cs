using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Sdx.Web
{
  public class DeviceUrl
  {
    public enum Device
    {
      Pc,
      Sp,
      Mb,
    }

    private Dictionary<Device, string> urls = new Dictionary<Device,string>();

    private string[] captureGroups;

    private Dictionary<Device, Dictionary<string, string>> queryMap = new Dictionary<Device, Dictionary<string, string>>();

    private Device currentDevice;

    private Dictionary<Device, Url> resultCache = new Dictionary<Device, Url>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentDevice">対象のページのデバイス</param>
    /// <param name="pc">対象のデバイスがPCじゃなかった場合URLのフォーマットを渡します。</param>
    /// <param name="sp">対象のデバイスがSPじゃなかった場合URLのフォーマットを渡します。</param>
    /// <param name="mb">対象のデバイスがMBじゃなかった場合URLのフォーマットを渡します。</param>
    /// <param name="regex">現在のURLからURLの変数を抽出する場合に正規表現を渡します。グループキャプチャの番号でフォーマットに指定してください（つまり最初のインデックスは1になります）。</param>
    public DeviceUrl(Device currentDevice, string pc = null, string sp = null, string mb = null, string regex = null)
    {
      this.currentDevice = currentDevice;

      if (currentDevice == Device.Pc && pc != null)
      {
        throw new ArgumentException("pc is not null, for current url device must be null");
      }
      else if (currentDevice == Device.Sp && sp != null)
      {
        throw new ArgumentException("sp is not null, for current url device must be null");
      }
      else if (currentDevice == Device.Mb && mb != null)
      {
        throw new ArgumentException("mb is not null, for current url device must be null");
      }

      urls[Device.Pc] = pc;
      urls[Device.Sp] = sp;
      urls[Device.Mb] = mb;

      //URLのプレイスフォルダを正規表現から作る
      var formatValues = new List<string>();
      if(regex != null)
      {
        var reg = new Regex(regex);
        var match = reg.Match(Sdx.Context.Current.Request.Url.PathAndQuery);
        if(!match.Success)
        {
          throw new ArgumentException("Not match regex to current url path " + Sdx.Context.Current.Request.Url.PathAndQuery);
        }

        for (int i = 0; i < match.Groups.Count; i++)
        {
          formatValues.Add(match.Groups[i].Value);
        }
      }

      captureGroups = formatValues.ToArray<string>();
    }

    /// <summary>
    /// URLを返す。コンストラクタでURLフォーマットをセットしなかったかつ現在のURLじゃなかった場合はNULLが返ります。
    /// </summary>
    public Sdx.Web.Url Pc
    {
      get
      {
        return BuildUrl(Device.Pc);
      }
    }

    /// <summary>
    /// URLを返す。コンストラクタでURLフォーマットをセットしなかったかつ現在のURLじゃなかった場合はNULLが返ります。
    /// </summary>
    public Sdx.Web.Url Mb
    {
      get
      {
        return BuildUrl(Device.Mb);
      }
    }

    /// <summary>
    /// URLを返す。コンストラクタでURLフォーマットをセットしなかったかつ現在のURLじゃなかった場合はNULLが返ります。
    /// </summary>
    public Sdx.Web.Url Sp
    {
      get
      {
        return BuildUrl(Device.Sp);
      }
    }

    private Sdx.Web.Url BuildUrl(Device device)
    {
      if (!resultCache.ContainsKey(device))
      {
        if (this.currentDevice == device)
        {
          resultCache[device] = new Url(Sdx.Context.Current.Request.Url.PathAndQuery);
        }
        else if (urls[device] != null)
        {
          var path = string.Format(urls[device], captureGroups.ToArray<string>());
          var result = new Url(path + Sdx.Context.Current.Request.Url.Query);
          if (queryMap.ContainsKey(device))
          {
            foreach (var kv in queryMap[device])
            {
              result.ReplaceParamKey(kv.Key, kv.Value);
            }
          }

          resultCache[device] = result;
        }
        else
        {
          resultCache[device] = null;
        } 
      }

      return resultCache[device];
    }

    private void AddQueryMap(Device device, string from, string to)
    {
      if (!queryMap.ContainsKey(device))
      {
        queryMap[device] = new Dictionary<string, string>();
      }

      queryMap[device][from] = to;
    }

    /// <summary>
    /// 現在のURLからクエリーのキーを置換したい場合にここにセット。
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void AddMbQueryMap(string from, string to)
    {
      AddQueryMap(Device.Mb, from, to);
    }

    /// <summary>
    /// 現在のURLからクエリーのキーを置換したい場合にここにセット。
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void AddSpQueryMap(string from, string to)
    {
      AddQueryMap(Device.Sp, from, to);
    }

    /// <summary>
    /// 現在のURLからクエリーのキーを置換したい場合にここにセット。
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void AddPcQueryMap(string from, string to)
    {
      AddQueryMap(Device.Pc, from, to);
    }

    /// <summary>
    /// 現在のページのデバイスが引数と一致するかを調べる。可変引数で複数指定し、どれかだったら、というチェックも可能。
    /// </summary>
    /// <param name="devices"></param>
    /// <returns></returns>
    public bool IsCurrent(params Device[] devices)
    {
      return devices.Any(dev => dev == currentDevice);
    }
  }
}
