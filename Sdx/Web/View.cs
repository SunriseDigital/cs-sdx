using System;
using System.Collections.Generic;
using System.Web;

namespace Sdx.Web
{
  public class View
  {
    private static String varsKey = "SDX.WEB.VIEW.VARS";
    public static Sdx.Collection.Holder Vars
    {
      get
      {
        if(!HttpContext.Current.Items.Contains(varsKey))
        {
          var holder = new Sdx.Collection.Holder();
          holder.StrictCheck = false;
          HttpContext.Current.Items[varsKey] = holder;
        }

        return (Sdx.Collection.Holder)HttpContext.Current.Items[varsKey];
      }
    }
  }
}
