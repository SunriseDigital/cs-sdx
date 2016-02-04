using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class scaffold_area_list : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    var scaffold = new Sdx.Scaffold.Manager(Test.Orm.LargeArea.Meta, Test.Db.CreateDb());
    scaffold.Title = "エリア";
    scaffold.EditPage = new Sdx.Web.Url("/scaffold/large-area/edit.aspx");
    scaffold.DisplayList
      .Add(Sdx.Scaffold.Param.Create()
        .Set("column", "name")
      ).Add(Sdx.Scaffold.Param.Create()
        .Set("column", "code")
      );
  }
}