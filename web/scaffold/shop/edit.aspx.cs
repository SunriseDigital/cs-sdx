using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class scaffold_area_edit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      var scaffold = Test.Scaffold.Shop.Create(Sdx.Db.Adapter.Manager.Get("main").Write);
      scaffold.BindToCurrentContext();
    }
}