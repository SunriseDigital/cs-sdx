using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class scaffold_area_list : System.Web.UI.Page
{
  Sdx.Scaffold.Manager scaffold;

  protected void Page_Load(object sender, EventArgs e)
  {
    Test.Scaffold.Area.Create();
  }
}