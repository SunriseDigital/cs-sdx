using System;
using System.IO;

public partial class form_test : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    Sdx.Context.Current.Debug.Log(Request.Form.GetValues("name"));
    Sdx.Context.Current.Debug.Log(Request.Form.GetValues("single"));
    Sdx.Context.Current.Debug.Log(Request.QueryString.GetValues("id"));

    StreamReader reader = new StreamReader(Request.InputStream);
    Sdx.Context.Current.Debug.Log(reader.ReadToEnd());
  }
}