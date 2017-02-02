using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdx.Web
{
  public class GoogleFriendry
  {
    private string pc;
    private string mb;
    private string sp;

    private string[] captureGroups;

    private Dictionary<string, Dictionary<string, string>> queryMap = new Dictionary<string, Dictionary<string, string>>();

    public GoogleFriendry(string pc = null, string sp = null, string mb = null, string regex = null)
    {
      this.pc = pc;
      this.sp = sp;
      this.mb = mb;

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

    public bool IsPcUrl
    {
      get
      {
        return this.pc == null;
      }
    }

    public bool IsMbUrl
    {
      get
      {
        return this.mb == null;
      }
    }

    public bool IsSpUrl
    {
      get
      {
        return this.sp == null;
      }
    }

    public Sdx.Web.Url RedirectUrl
    {  
      get
      {
        Sdx.Context.Current.Debug.Log(Sdx.Context.Current.UserAgent.Device.IsSp);
        if(Sdx.Context.Current.UserAgent.Device.IsPc)
        {
          if(IsPcUrl)
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
          if(IsSpUrl)
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
          if (IsMbUrl)
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
        return BuildUrl(pc, IsPcUrl);
      }
    }

    public Sdx.Web.Url MbUrl
    {
      get
      {
        return BuildUrl(mb, IsMbUrl);
      }
    }

    public Sdx.Web.Url SpUrl
    {
      get
      {
        return BuildUrl(sp, IsSpUrl);
      }
    }

    private Sdx.Web.Url BuildUrl(string url, bool isCurrent)
    {
      if (isCurrent)
      {
        return new Url(Sdx.Context.Current.Request.Url.PathAndQuery);
      }
      else
      {
        var path = string.Format(url, captureGroups.ToArray<string>());
        var result = new Url(path + Sdx.Context.Current.Request.Url.Query);
        if(queryMap.ContainsKey(url))
        {
          foreach(var kv in queryMap[url])
          {
            result.ReplaceParamKey(kv.Key, kv.Value);
          }
        }
        return result;
      }
    }

    public void AddMbQueryMap(string from, string to)
    {
      if(!queryMap.ContainsKey(mb))
      {
        queryMap[mb] = new Dictionary<string, string>();
      }

      queryMap[mb][from] = to;
    }

    public void AddSpQueryMap(string from, string to)
    {
      if (!queryMap.ContainsKey(sp))
      {
        queryMap[sp] = new Dictionary<string, string>();
      }

      queryMap[sp][from] = to;
    }

    public void AddPcQueryMap(string from, string to)
    {
      if (!queryMap.ContainsKey(pc))
      {
        queryMap[pc] = new Dictionary<string, string>();
      }

      queryMap[pc][from] = to;
    }
  }
}
