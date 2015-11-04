using System;
using System.IO;

public partial class form_test : System.Web.UI.Page
{
  protected Sdx.Html.Form form;
  protected void Page_Load(object sender, EventArgs e)
  {
    form = new Sdx.Html.Form();
    form.SetActionToCurrent();

    var inputText = new Sdx.Html.InputText();
    inputText.Name = "input_text";
    form.SetElement(inputText);

    var select = new Sdx.Html.Select();
    select.Name = "select";
    form.SetElement(select);

    Sdx.Html.Option option;

    option = new Sdx.Html.Option();
    option.Tag.Attr["value"] = "10";
    option.Text = "foo";
    select.AddOption(option, "group1");

    option = new Sdx.Html.Option();
    option.Tag.Attr["value"] = "12";
    option.Text = "bar";
    select.AddOption(option, "group2");

    Sdx.Context.Current.Debug.Log(Request.Form, "POST");
    Sdx.Context.Current.Debug.Log(Request.QueryString, "GET");

    StreamReader reader = new StreamReader(Request.InputStream);
    Sdx.Context.Current.Debug.Log(reader.ReadToEnd(), "Request.InputStream");
  }
}