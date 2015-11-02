using System;
using System.IO;

public partial class form_test : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    Sdx.Context.Current.Debug.Log(Request.Form, "POST");
    Sdx.Context.Current.Debug.Log(Request.QueryString, "GET");

    StreamReader reader = new StreamReader(Request.InputStream);
    Sdx.Context.Current.Debug.Log(reader.ReadToEnd(), "Request.InputStream");
  }
}