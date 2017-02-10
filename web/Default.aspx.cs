using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    foreach(var name in new string[]{"sqlserver", "mysql", "main"})
    {
      var db = Sdx.Db.Adapter.Manager.Get(name).Read;
      using (var conn = db.CreateConnection())
      {
        conn.Open();
      }
    }
  }
}