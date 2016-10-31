using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace Sdx.Web
{
  public static class Helper
  {
    public static void JsonResponse(object values)
    {
      //IEがapplication/jsonに対応していないので
      if (Request.Headers["Accept"] != null && Request.Headers["Accept"].Contains("application/json"))
      {
        Response.ContentType = "application/json; charset=utf-8";
      }
      else
      {
        Response.ContentType = "text/plain; charset=utf-8";
      }

      Response.Write(Sdx.Util.Json.Encoder(values));
    }

    public static HttpRequest Request
    {
      get
      {
        return HttpContext.Current.Request;
      }
    }

    public static HttpResponse Response
    {
      get
      {
        return HttpContext.Current.Response;
      }
    }

    public static HttpServerUtility Server
    {
      get
      {
        return HttpContext.Current.Server;
      }
    }

    public static bool HandleMaxRequestLengthWithJson()
    {
      var url = WebConfigurationManager.GetSection("handleMaxRequestLengthWithJson") as Sdx.Configuration.DictionaryListSection;
      if (url != null)
      {
        if (url.List.Any(elem => elem.Attributes["path"] == Request.Path))
        {
          var serverError = Server.GetLastError() as HttpException;
          //TODO 他言語時のエラーメッセージチェックの対応。
          if (serverError.InnerException.Message == "要求の長さの最大値を超えました。")
          {
            Server.ClearError();
            Response.StatusCode = 500;
            HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            Sdx.Web.Helper.JsonResponse(new Dictionary<string, string>() {
              { "type", "MaxRequestLength" },
              { "maxLength", section.MaxRequestLength.ToString()}
            });
            return true;
          }
        }
      }

      return false;
    }

    private static List<string> trustedIPList;

    public static string ClientIPAddressByString
    {
      get
      {
        string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        if (!string.IsNullOrEmpty(ipAddress))
        {
          string[] addresses = ipAddress.Split(',');
          if (addresses.Length != 0)
          {
            return addresses[0];
          }
        }

        return Request.ServerVariables["REMOTE_ADDR"];
      }
    }


    public static bool IsTrustedIPRequest
    {
      get
      {
        if(Request.IsLocal)
        {
          return true;
        }

        if(trustedIPList == null)
        {
          trustedIPList = new List<string>();
          var strList = System.Web.Configuration.WebConfigurationManager.AppSettings["TrustedIPAddressList"];
          if(strList != null)
          {
            trustedIPList.AddRange(strList.Split(',').Select(str => str.Trim()));
          }
        }

        return trustedIPList.Contains(ClientIPAddressByString);
      }
    }
  }
}
