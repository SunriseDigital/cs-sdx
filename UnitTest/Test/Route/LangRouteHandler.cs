using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;

namespace Test.Route
{
  public class LangRouteHandler : IRouteHandler
  {
    public LangRouteHandler(string virtualPath)
    {
      this.VirtualPath = virtualPath;
    }

    public string VirtualPath { get; set; }

    public IHttpHandler GetHttpHandler(RequestContext requestContext)
    {
      String vPath = VirtualPath
        .Replace("{controller}", requestContext.RouteData.Values["controller"].ToString())
        .Replace("{action}", requestContext.RouteData.Values["action"].ToString())
        ;
      Sdx.Context.Current.Culture = new CultureInfo(requestContext.RouteData.Values["lang"].ToString());
      return BuildManager.CreateInstanceFromVirtualPath(vPath, typeof(System.Web.UI.Page)) as IHttpHandler;
    }
  }
}
