using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Sdx.Web
{
  public abstract class DeviceRedirectHttpModule : IHttpModule
  {
    private string mobileUa = @"DoCoMo|UP.Browser|SoftBank|WILLCOM";

    private string smartPhoneUa = @"iPhone|Android.*Mobile|Windows.*Phone";

    private void Application_BeginRequest(object source, EventArgs a)
    {
      //var deviceTable = Sdx.Web.DeviceTable.Current;
      //string url = "";
      
      //if(deviceTable != null)
      //{
      //  string userAgent = HttpContext.Current.Request.UserAgent;
      
      //  if (Regex.IsMatch(userAgent, smartPhoneUa))
      //  {
      //    url = deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Sp);
      //  }
      //  else if (Regex.IsMatch(userAgent, mobileUa))
      //  {
      //    url = deviceTable.GetUrl(Sdx.Web.DeviceTable.Device.Mb);
      //  }
      //}

      //if (!string.IsNullOrEmpty(url) && url != HttpContext.Current.Request.RawUrl)
      //{
      //  HttpContext.Current.Response.Redirect(url, false);
      //}
      var currentPageDevice = DetectUrlDevice(HttpContext.Current.Request.RawUrl);

      var settingPath = GetSettingPath();
      Sdx.Web.DeviceTable.Current = new Sdx.Web.DeviceTable(currentPageDevice, HttpContext.Current.Request.RawUrl, settingPath);

      var currentUserAgentDevice = DetectUserAgentDevice();
      if (currentPageDevice == currentUserAgentDevice)
      {
        return;
      }

      //GetUrlが呼ばれて初めてsettingを読み込んで、pageの解析を行う。
      //結果はメモリにキャッシュしてください。
      var url = Sdx.Web.DeviceTable.Current.GetUrl(currentUserAgentDevice);
      if (url != null)
      {
        HttpContext.Current.Response.Redirect(url, false);
      }
    }

    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(Application_BeginRequest);
    }

    public void Dispose() { }

    public abstract Sdx.Web.DeviceTable.Device DetectUrlDevice(string url);

    public Sdx.Web.DeviceTable.Device DetectUserAgentDevice()
    {
      return Sdx.Web.DeviceTable.Device.Pc;
    }

    public abstract string GetSettingPath();
  }
}
