using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class scaffold_shop_list : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    (list as dynamic).Scaffold = Test.Scaffold.ShopImage.Create(Sdx.Db.Adapter.Manager.Get("main").Write);
  }
}