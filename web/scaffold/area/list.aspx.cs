﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class scaffold_area_list : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      var scaffold = new Sdx.Web.Scaffold<Test.Orm.Area>(Test.Db.CreateSqlServer(), "area");
    }
}