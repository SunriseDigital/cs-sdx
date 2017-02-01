using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public class GoogleFriendry
  {
    private string pc;
    private string mb;
    private string sp;

    public GoogleFriendry(string pc = null, string sp = null, string mb = null)
    {
      this.pc = pc;
      this.sp = sp;
      this.mb = mb;
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
        if(IsPcUrl)
        {
          return Sdx.Context.Current.Request.Path;
        }
        else
        {
          return pc;
        }
      }
    }

    public string MbUrl
    {
      get
      {
        if (IsMbUrl)
        {
          return Sdx.Context.Current.Request.Path;
        }
        else
        {
          return mb;
        }
      }
    }

    public string SpUrl
    {
      get
      {
        if (IsSpUrl)
        {
          return Sdx.Context.Current.Request.Path;
        }
        else
        {
          return sp;
        }
      }
    }
  }
}
