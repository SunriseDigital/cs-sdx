using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Web
{
  public class UserControl : System.Web.UI.UserControl
  {
    public static UserControl Find(System.Web.UI.Page page, string controlId)
    {
      return (UserControl)page.FindControl(controlId);
    }

    private Holder vars = new Holder();

    public Holder Vars
    {
      get
      {
        return this.vars;
      }
    }
  }
}
