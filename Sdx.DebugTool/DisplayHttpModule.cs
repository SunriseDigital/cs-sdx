using System;
using System.Web;
using System.Diagnostics;

namespace Sdx.DebugTool
{
  class DisplayHttpModule : IHttpModule
  {
    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(app_BeginRequest);
    }

    public void Dispose() { }

    void app_BeginRequest(object sender, EventArgs a)
    {
      Debug.initRequestTimer();
      HttpContext.Current.Response.Filter = new Sdx.DebugTool.DisplayResponseFilter(HttpContext.Current.Response.Filter);
    }
  }
}
