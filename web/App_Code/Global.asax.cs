using System;
using System.Web.Routing;

public class Global : System.Web.HttpApplication
{
  protected void Application_Start(Object sender, EventArgs e)
  {
    RouteTable.Routes.MapPageRoute(
      "FormCurrent",
      "form/current",
      "~/_routes/form/current.aspx"
    );
  }

  protected void Application_BeginRequest(Object sender, EventArgs e)
  {
    
  }
}