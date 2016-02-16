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
  }

  protected void Application_BeginRequest(Object sender, EventArgs e)
  {

  }

  protected void Application_Error(object sender, EventArgs e)
  {
    var serverError = Server.GetLastError() as HttpException;
    Sdx.Context.Current.Debug.Log(serverError);
    //Console.WriteLine(serverError);
    // An error has occured on a .Net page.
    //var serverError = Server.GetLastError() as HttpException;

    //if (null != serverError)
    //{
    //  int errorCode = serverError.GetHttpCode();

    //  if (404 == errorCode)
    //  {
    //    Server.ClearError();
    //    Server.Transfer("/Errors/404.aspx");
    //  }
    //}
  }
}