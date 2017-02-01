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

    public string RedirectUrl
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

    public string PcUrl
    {
      get
      {
        return BuildUrl(pc, IsPcUrl);
      }
    }

    public string MbUrl
    {
      get
      {
        return BuildUrl(mb, IsMbUrl);
      }
    }

    public string SpUrl
    {
      get
      {
        return BuildUrl(sp, IsSpUrl);
      }
    }

    private string BuildUrl(string url, bool isCurrent)
    {
      if (isCurrent)
      {
        return Sdx.Context.Current.Request.Url.PathAndQuery;
      }
      else
      {
        return string.Format(url, captureGroups.ToArray<string>());
      }
    }
  }
}
