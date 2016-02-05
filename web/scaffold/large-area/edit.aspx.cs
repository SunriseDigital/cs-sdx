﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class scaffold_area_edit : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      var scaffold = new Sdx.Scaffold.Manager(Test.Orm.LargeArea.Meta, Test.Db.CreateDb());
      scaffold.Title = "大エリア";
      scaffold.ListPage = new Sdx.Web.Url("/scaffold/large-area/list.aspx");
      scaffold.FormList
        .Add(Sdx.Scaffold.Param.Create()
          .Set("column", "id")
          .Set("label", "ID")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "name")
          .Set("label", "名称")
        ).Add(Sdx.Scaffold.Param.Create()
          .Set("column", "code")
          .Set("label", "コード")
        );
    }
}