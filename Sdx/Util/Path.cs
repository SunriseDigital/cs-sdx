using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Sdx.Util
{
  public static class Path
  {
    private const string NeedsFullSourceKey = "Sdx.Util.Path.NeedsFullSourceKey";

    private static bool NeedsFullSource
    {
      get
      {
        if (!Sdx.Context.Current.Vars.ContainsKey(NeedsFullSourceKey))
        {
          Sdx.Context.Current.Vars[NeedsFullSourceKey] = false;
          if(Sdx.Context.Current.IsDebugMode)
          {
            Sdx.Context.Current.Vars[NeedsFullSourceKey] = true;
          }
          else
          {
            var cookie = HttpContext.Current.Request.Cookies["sdx_full_source"];
            if (cookie != null && cookie.Value == "1")
            {
              Sdx.Context.Current.Vars[NeedsFullSourceKey] = true; 
            }
          }
        }

        return (bool)Sdx.Context.Current.Vars[NeedsFullSourceKey];
      }
    }

    /// <summary>
    /// .min.js|.min.cssの拡張子のついたパスを渡すと、以下の条件の時minを取り除いたパスを返します。
    /// * Cookieに`sdx_full_source=1`が入っていたとき。
    /// * <see cref="Sdx.Context.Current.IsDebugMode"/>がtrueの時
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string AutoMin(string path)
    {
      
      if(NeedsFullSource)
      {
        var chunk = path.Split('.');
        var minPos = chunk.Length - 2;
        if (minPos >= 0 && chunk[minPos] == "min")
        {
          return System.String.Join(".", chunk.Where((val, idx) => idx != minPos));
        }
        else
        {
          return path;
        }
      }
      else
      {
        return path;
      }
    }

    public static string MapWebPath(string fullFilePath)
    {
      //TODO WebPathが含まれていなかったら例外。
      return fullFilePath.Replace(HttpContext.Current.Server.MapPath("~/"), @"\").Replace(@"\", "/");
    }
  }
}
