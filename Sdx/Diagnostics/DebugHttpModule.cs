using System;
using System.Web;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sdx.Diagnostics
{
  public class DebugHttpModule : IHttpModule
  {
    private const string LogBlockFormat = @"
<div style=""background-color: #ebebeb; border-radius: 5px; padding: 0;margin-bottom: 20px;"">
	<div style = ""background-color: #808080; color: #fff; font-weight: bold; border-radius: 5px 5px 0 0; padding: 5px 10px;"" >
		{0}
	</div>
  <div style=""padding: 20px; margin: 0;"">{1}</div>
</div>";

    private const string QueryBlockFormat = @"
<div style=""margin-bottom: 20px; line-height: 16px;"">
  <div style=""margin-bottom: 5px; clear: both;"">
        <span style=""color: red;"">[{0} sec]</span>&nbsp;
        <span>{1}</span>
        <span style=""color: gray; font-size: 11px; float: right;"">{2}</span>
  </div>
  <div style=""margin-bottom: 5px;"">{3}</div>
  <div>{4}</div>
</div>
";

    private const string SubTitleDiv = @"<div style=""background-color: #D5D5D5;padding: 2px 5px; border-radius: 3px; margin-bottom: 5px;"">";

    public void Init(HttpApplication application)
    {
      application.BeginRequest += new EventHandler(Application_BeginRequest);
      application.EndRequest += new EventHandler(this.Application_EndRequest);
    }

    public void Dispose() { }

    private const string HtmlWriteContextKey = "Sdx.Diagnostics.DebugHttpModule.HtmlWriterContextKey";

    private void Application_BeginRequest(object source, EventArgs a)
    {
      //start timer
      Sdx.Context.Current.Timer.Start();

      Sdx.Context.Current.Debug.Out = new Diagnostics.DebugHtmlWriter();

      //debug mode
      var cookie = HttpContext.Current.Request.Cookies["sdx_debug_mode"];
      if (cookie != null && cookie.Value == "1")
      {
        Sdx.Context.Current.IsDebugMode = true;
      }
    }

    private void Application_EndRequest(object source, EventArgs a)
    {
      //Debug.Log
      HttpApplication application = (HttpApplication)source;
      HttpContext context = application.Context;

      WriteLogs(context);
    }

    private void WriteLogs(HttpContext context)
    {
      var debugString = new StringBuilder();

      debugString.Append("<div style=\"padding: 10px; font-size: 12px; margin: 0; clear: both;\">");
      this.AppendDebugLogs(debugString);
      if(Sdx.Context.Current.IsDebugMode)
      {
        this.AppendDbQueryLogs(debugString);
      }
      debugString.Append("</div>");

      context.Response.Write(debugString.ToString());
    }

    private void AppendDbQueryLogs(StringBuilder debugString)
    {
      if (Sdx.Context.Current.DbProfiler == null)
      {
        return;
      }

      long totalElapsed = 0;
      Sdx.Db.Sql.Log slowQuery = null;
      var queryString = new StringBuilder();

      if(Sdx.Context.Current.DbProfiler.Queries.Count > 0)
      {
        queryString.Append(SubTitleDiv);
        queryString.Append("all queries");
        queryString.Append("</div>");
      }

      queryString.Append(@"<ol style=""margin: 0; padding: 0; list-style-position: inside;"">");
      Sdx.Context.Current.DbProfiler.Queries.ForEach(query => {
        queryString.Append("<li>");
        queryString.Append(this.buildQueryString(query));
        queryString.Append("</li>");

        //calc total elapsed time
        totalElapsed += query.ElapsedTime;

        //detect slowest query;
        if(slowQuery == null || query.ElapsedTime > slowQuery.ElapsedTime)
        {
          slowQuery = query;
        }
      });
      queryString.Append("</ol>");

      var slowQueryString = new StringBuilder();
      if(slowQuery != null)
      {
        slowQueryString.Append(SubTitleDiv);
        slowQueryString.Append("max const query");
        slowQueryString.Append("</div>");
        slowQueryString.Append(this.buildQueryString(slowQuery));
      }

      debugString.Append(String.Format(
        LogBlockFormat,
        Sdx.Context.Current.DbProfiler.Queries.Count + " queries " + Sdx.Diagnostics.Debug.FormatStopwatchTicks(totalElapsed) + " sec",
        slowQueryString.ToString() + queryString.ToString()
      ));
    }

    private string buildQueryString(Sdx.Db.Sql.Log query)
    {
      return String.Format(
        QueryBlockFormat,
        query.FormatedElapsedTime,
        query.Comment,
        query.Adapter.ToString(),
        query.CommandText,
        query.FormatedParameters
      );
    }

    private void AppendDebugLogs(StringBuilder debugString)
    {
      if(!(Sdx.Context.Current.Debug.Out is DebugHtmlWriter))
      {
        return;
      }

      var writer = (DebugHtmlWriter)Sdx.Context.Current.Debug.Out;
      if(writer.Builder.Length == 0)
      {
        return;
      }

      debugString.Append(String.Format(
        LogBlockFormat,
        "Debug.Log",
        "<pre style=\"margin: 0;\">" + writer.Builder.ToString() + "</pre>"
      ));
    }
  }
}
