using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Web
{
  public class UserControl : System.Web.UI.UserControl
  {
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
