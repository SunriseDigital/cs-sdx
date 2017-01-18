using System;
using System.Collections.Generic;
using System.IO;
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
      var currentPageDevice = DetectUrlDevice(HttpContext.Current.Request.RawUrl);

      var settingPath = GetSettingPath();

      Sdx.Web.DeviceTable.Current = new Sdx.Web.DeviceTable(currentPageDevice, HttpContext.Current.Request.RawUrl, settingPath);

      var currentUserAgentDevice = DetectUserAgentDevice();

      if (currentPageDevice == currentUserAgentDevice)
      {
        return;
      }

      var url = Sdx.Web.DeviceTable.Current.GetUrl(currentUserAgentDevice);

      if (!string.IsNullOrEmpty(url))
      {
        HttpContext.Current.Response.Redirect(url, false);
      }
    }

    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(Application_BeginRequest);
    }

    public void Dispose() { }

    protected abstract Sdx.Web.DeviceTable.Device DetectUrlDevice(string url);

    private Sdx.Web.DeviceTable.Device DetectUserAgentDevice()
    {
      string userAgent = HttpContext.Current.Request.UserAgent;

      Regex reg = new Regex(smartPhoneUa, RegexOptions.Compiled);
      if (reg.IsMatch(userAgent))
      {
        return DeviceTable.Device.Sp;
      }

      reg = new Regex(mobileUa, RegexOptions.Compiled);
      if (reg.IsMatch(userAgent))
      {
        return DeviceTable.Device.Mb;
      }

      return DeviceTable.Device.Pc;
    }

    protected abstract string GetSettingPath();
  }
}
