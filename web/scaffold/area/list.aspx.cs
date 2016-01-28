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
    var scaffold = new Sdx.Scaffold.Manager<Test.Orm.Area>(Test.Db.CreateSqlServer(), "area");
    scaffold.Title = "エリア";
    scaffold.ListColumns = Sdx.Collection.HolderList.Create()
      .Add(Sdx.Collection.Holder.Create()
        .Set("column", "name")
        .Set("foo", "var")
      ).Add(Sdx.Collection.Holder.Create()
        .Set("column", "code")
        .Set("foo", "var")
      );
    //scaffold.ListColumns = new List<Dictionary<string, string>>() { 
    //    new Dictionary<string, string>{
    //      {"column",  "name"}
    //    },
    //    new Dictionary<string, string>{
    //      {"column",  "code"}
    //    }
    //  };
  }
}