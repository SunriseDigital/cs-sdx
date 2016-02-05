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
    scaffold = new Sdx.Scaffold.Manager(Test.Orm.Area.Meta, Test.Db.CreateDb());
    scaffold.Title = "エリア";
    scaffold.EditPage = new Sdx.Web.Url("/scaffold/area/edit.aspx");

    scaffold.DisplayList
      .Add(Sdx.Scaffold.Param.Create()
        .Set("column", "name")
      ).Add(Sdx.Scaffold.Param.Create()
        .Set("column", "code")
      );

    //scaffold.Group = new Sdx.Scaffold.Group.StaticClass("large_area_id", "GetName", "GetList");
    scaffold.Group = new Sdx.Scaffold.Group.TableMeta("large_area_id", Test.Orm.LargeArea.Meta, "name", "FetchAllDefaultOrdered");
  }
}