﻿using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using System.Linq;
using System.Collections.Generic;

public class Global : System.Web.HttpApplication
{
  protected void Application_Start(Object sender, EventArgs e)
  {
    RouteTable.Routes.Add(new Route(
      "{lang}/{controller}/{*action}",
      new RouteValueDictionary { { "lang", "ja" }, { "controller", "default" }, { "action", "default" } },
      new RouteValueDictionary { { "lang", "ja|en" } },
      new Test.Route.LangRouteHandler("~/{controller}/{action}")
    ));

    //DB Adapter
    var settings = WebConfigurationManager.GetSection("sdxDatabaseConnections") as Sdx.Configuration.DictionaryListSection;
    foreach (var elem in settings.Items)
    {
      Sdx.Db.Adapter.Manager.Add(elem.Attributes, WebConfigurationManager.ConnectionStrings, WebConfigurationManager.AppSettings);
    }
  }

  protected void Application_BeginRequest(Object sender, EventArgs e)
  {
    Sdx.Context.Current.HttpErrorHandler.SetHandler(404, () => 
    {
      HttpContext.Current.Response.TrySkipIisCustomErrors = true;
      HttpContext.Current.Response.StatusCode = 404;
      HttpContext.Current.Server.Transfer("/_error/404.aspx");
    });
  }

  protected void Application_Error(object sender, EventArgs e)
  {
    Sdx.Web.Helper.HandleMaxRequestLengthWithJson();

    //var serverError = Server.GetLastError() as HttpException;

    //if (null != serverError)
    //{
    //  int errorCode = serverError.GetHttpCode();
    //  if (404 == errorCode)
    //  {
    //    Server.ClearError();
    //    Server.Transfer("/_error/404.aspx");
    //  }
    //}
  }

  private Dictionary<string, string> BuildDictionaryFromException(Exception exception)
  {
    var dic = new Dictionary<string, string>();
    dic["type"] = exception.GetType().ToString();
    dic["message"] = exception.Message;
    dic["source"] = exception.Source;
    return dic;
  }
}