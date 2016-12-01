using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Sdx.Web
{
  public class DeviceRedirectHttpModule : IHttpModule
  {
    private string mobileUa = @"DoCoMo|UP.Browser|SoftBank|WILLCOM";

    private string smartPhoneUa = @"iPhone|Android.*Mobile|Windows.*Phone";

    private void Application_BeginRequest(object source, EventArgs a)
    {
      var deviceTable = Sdx.Web.DeviceTable.Current;
      string url = "";
      
      if(deviceTable != null)
      {
        string userAgent = HttpContext.Current.Request.UserAgent;
        Sdx.Context.Current.Debug.Log(userAgent);        
        if (Regex.IsMatch(userAgent, smartPhoneUa))
        {
          url = deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp);
        }
        else if (Regex.IsMatch(userAgent, mobileUa))
        {
          url = deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb);
        }
      }

      if (!string.IsNullOrEmpty(url) && url != HttpContext.Current.Request.RawUrl)
      {
        HttpContext.Current.Response.Redirect(url, false);
      }
    }

    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(Application_BeginRequest);
    }

    public void Dispose() { }
  }
}
