using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Web
{
  public static class Util
  {
    public static UserControl FindControl(System.Web.UI.Page page, string controlId)
    {
      return (UserControl)page.FindControl(controlId);
    }
  }
}
