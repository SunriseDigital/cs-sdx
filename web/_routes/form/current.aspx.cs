﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _routes_form_current : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    var form = new Sdx.Html.Form();
    form.SetActionToCurrent();

    Sdx.Web.View.Vars.Set("form", form);
  }
}