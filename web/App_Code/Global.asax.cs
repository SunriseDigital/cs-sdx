using System;
using System.Web;
using System.Web.Routing;

public class Global : System.Web.HttpApplication
{
  protected void Application_Start(Object sender, EventArgs e)
  {
    //RouteTable.Routes.MapPageRoute(
    //  "FormCurrent",
    //  "form/current",
    //  "~/_routes/form/current.aspx"
    //);

    RouteTable.Routes.Add(new Route(
      "{lang}/{controller}/{*action}",
      new RouteValueDictionary { { "lang", "ja" }, { "controller", "default" }, { "action", "default" } },
      new RouteValueDictionary { { "lang", "ja|en" } },
      new Test.Route.LangRouteHandler("~/{controller}/{action}")
    ));

    Test.Db.Adapter.SetupManager();
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

  //protected void Application_Error(object sender, EventArgs e)
  //{
  //  var serverError = Server.GetLastError() as HttpException;

  //  if (null != serverError)
  //  {
  //    int errorCode = serverError.GetHttpCode();
  //    if (404 == errorCode)
  //    {
  //      Server.ClearError();
  //      Server.Transfer("/_error/404.aspx");
  //    }
  //  }
  //}
}