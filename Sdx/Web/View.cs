using System;
using System.Collections.Generic;
using System.Web;

namespace Sdx.Web
{
  public class View
  {
    private static String varsKey = "SDX.WEB.VIEW.VARS";
    public static Holder Vars
    {
      get
      {
        if(!HttpContext.Current.Items.Contains(varsKey))
        {
          HttpContext.Current.Items[varsKey] = new Holder();
        }

        return (Holder)HttpContext.Current.Items[varsKey];
      }
    }
  }
}
