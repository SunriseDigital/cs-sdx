using System;
using System.Web;
using System.Diagnostics;
using System.Collections.Generic;

namespace Sdx.Diagnostics
{
  public class DebugHttpModule : IHttpModule
  {
    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(Application_BeginRequest);
      application.EndRequest += new EventHandler(this.Application_EndRequest);
    }

    public void Dispose() { }

    private void Application_BeginRequest(object source, EventArgs a)
    {
      //start timer
      Sdx.Context.Current.Timer.Start();

      //debug mode
      if(HttpContext.Current.Request.Cookies["sdx_debug_mode"].Value == "1")
      {
        Sdx.Context.Current.IsDebugMode = true;
      }
    }

    private void Application_EndRequest(object source, EventArgs a)
    {

      //Debug.Log
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

      var debugString = "";
      debugString += "<div style=\"padding: 10px; font-size: 12px; margin: 0; clear: both;\">";
      Int64 prevElapsed = -1;
      foreach (Dictionary<String, Object> dic in Debug.Logs)
      {
        var totalElapsed = (Int64)dic["elapsedTicks"];
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
        "<div style=\"background-color: #ebebeb; border-radius: 5px; padding: 0;margin-bottom: 20px;\">" +
          "<div style=\"    background-color: #808080; color: #fff; font-weight: bold; border-radius: 5px 5px 0 0; padding: 5px 10px;\">" +
            "[" + formatTicks(currentElapsed) + "/" + formatTicks(totalElapsed) + "]" + title +
          "</div>" +
          "<pre style=\"padding: 10px; margin: 0;\">" + value + "</pre>" +
        "</div>";
    }

    private String formatTicks(long ticks)
    {
      return Debug.FormatStopwatchTicks(ticks);
    }
  }
}
