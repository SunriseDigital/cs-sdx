using System;
using System.Web;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sdx.DebugTool
{
  class DisplayHttpModule : IHttpModule
  {
    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(Application_BeginRequest);
      application.EndRequest += new EventHandler(this.Application_EndRequest);
    }

    public void Dispose() { }

    private void Application_BeginRequest(object source, EventArgs a)
    {
      Debug.initRequestTimer();
    }

    private void Application_EndRequest(object source, EventArgs a)
    {
      HttpApplication application = (HttpApplication)source;
      HttpContext context = application.Context;

      writeDebugLogs(context);
    }

    private void writeDebugLogs(HttpContext context)
    {
      if(Debug.Logs.Count == 0)
      {
        return;
      }

      var debugString = "<div class=\"sdx-debug-wrapper\">";
      Int64 prevElapsed = -1;
      foreach (Dictionary<String, Object> dic in Debug.Logs)
      {
        var totalElapsed = (Int64)dic["elapsedMsec"];
        Int64 currentElapsed = 0;
        if (prevElapsed != -1)
        {
          currentElapsed = totalElapsed - prevElapsed;
        }

        debugString += WrapTag(
          Debug.Dump(dic["value"]),
          dic["title"] as String,
          totalElapsed,
          currentElapsed
        );

        prevElapsed = totalElapsed;
      }
      debugString += "</div>";

      context.Response.Write(debugString);
    }

    private String WrapTag(String value, String title, Int64 totalElapsed, Int64 currentElapsed)
    {
      DateTime now = DateTime.Now;
      return
        "<div class=\"sdx-debug-item\">" +
          "<div class=\"sdx-debug-title\">" +
            "[" + currentElapsed / 1000.0 + "/" + totalElapsed / 1000.0 + "]" + title +
          "</div>" +
          "<pre class=\"sdx-debug-value\">" + value + "</pre>" +
        "</div>";
    }
  }
}
