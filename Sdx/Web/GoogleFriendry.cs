﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdx.Web
{
  public class GoogleFriendry
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

    private Device currentUrlDevice;

    private Dictionary<Device, Url> resultCache = new Dictionary<Device, Url>();

    public GoogleFriendry(Device device, string pc = null, string sp = null, string mb = null, string regex = null)
    {
      currentUrlDevice = device;

      if (currentUrlDevice == Device.Pc && pc != null)
      {
        throw new ArgumentException("pc is not null, for current url device must be null");
      }
      else if (currentUrlDevice == Device.Sp && sp != null)
      {
        throw new ArgumentException("sp is not null, for current url device must be null");
      }
      else if (currentUrlDevice == Device.Mb && mb != null)
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

    public Sdx.Web.Url RedirectUrl
    {  
      get
      {
        if(Sdx.Context.Current.UserAgent.Device.IsPc)
        {
          if(currentUrlDevice == Device.Pc)
          {
            return null;
          }
          else
          {
            return PcUrl;
          }
        }
        else if(Sdx.Context.Current.UserAgent.Device.IsSp)
        {
          if (currentUrlDevice == Device.Sp)
          {
            return null;
          }
          else
          {
            return SpUrl;
          }
        }
        else if (Sdx.Context.Current.UserAgent.Device.IsMb)
        {
          if (currentUrlDevice == Device.Mb)
          {
            return null;
          }
          else
          {
            return MbUrl;
          }
        }
        else
        {
          return null;
        }
      }
    }

    public Sdx.Web.Url PcUrl
    {
      get
      {
        return BuildUrl(Device.Pc);
      }
    }

    public Sdx.Web.Url MbUrl
    {
      get
      {
        return BuildUrl(Device.Mb);
      }
    }

    public Sdx.Web.Url SpUrl
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
        if (currentUrlDevice == device)
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

    public void AddMbQueryMap(string from, string to)
    {
      AddQueryMap(Device.Mb, from, to);
    }

    public void AddSpQueryMap(string from, string to)
    {
      AddQueryMap(Device.Sp, from, to);
    }

    public void AddPcQueryMap(string from, string to)
    {
      AddQueryMap(Device.Pc, from, to);
    }
  }
}
