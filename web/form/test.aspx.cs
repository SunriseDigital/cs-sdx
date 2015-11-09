using System;
using System.IO;

public partial class form_test : System.Web.UI.Page
{
  protected Sdx.Html.Form form;
  protected void Page_Load(object sender, EventArgs e)
  {
    form = new Sdx.Html.Form();

    //Text
    var inputText = new Sdx.Html.InputText();
    inputText.Name = "input_text";
    inputText.AddValidator(new Sdx.Validation.NotEmpty());
    form.SetElement(inputText);

    //Select
    var select = new Sdx.Html.Select();
    select.Name = "select";
    select.AddValidator(new Sdx.Validation.NotEmpty());
    form.SetElement(select);

    Sdx.Html.Option option;

    option = new Sdx.Html.Option();
    option.Text = "Select this !!";
    option.Tag.Attr["value"] = "";
    select.AddOption(option);

    option = new Sdx.Html.Option();
    option.Tag.Attr["value"] = "10";
    option.Text = "foo";
    select.AddOption(option, "group1");

    option = new Sdx.Html.Option();
    option.Tag.Attr["value"] = "12";
    option.Text = "bar";
    select.AddOption(option, "group2");

    //Checkbox
    var checkList = new Sdx.Html.CheckableGroup();
    checkList.Name = "check_list";
    checkList.AddValidator(new Sdx.Validation.NotEmpty());
    form.SetElement(checkList);

    Sdx.Html.CheckBox checkbox;

    checkbox = new Sdx.Html.CheckBox();
    checkbox.Tag.Attr["value"] = "20";
    checkbox.Tag.Attr["id"] = "check_list_20";
    checkList.AddCheckable(checkbox, "いち");

    checkbox = new Sdx.Html.CheckBox();
    checkbox.Tag.Attr["value"] = "21";
    checkbox.Tag.Attr["id"] = "check_list_21";
    checkList.AddCheckable(checkbox, "にい");

    checkbox = new Sdx.Html.CheckBox();
    checkbox.Tag.Attr["value"] = "22";
    checkbox.Tag.Attr["id"] = "check_list_22";
    checkList.AddCheckable(checkbox, "さん");

    //Radio
    var radios = new Sdx.Html.CheckableGroup();
    radios.Name = "radios";
    radios.AddValidator(new Sdx.Validation.NotEmpty());
    form.SetElement(radios);

    Sdx.Html.Radio radio;

    radio = new Sdx.Html.Radio();
    radio.Tag.Attr["value"] = "30";
    radio.Tag.Attr["id"] = "radios_30";
    radios.AddCheckable(radio, "らじお　いち");

    radio = new Sdx.Html.Radio();
    radio.Tag.Attr["value"] = "31";
    radio.Tag.Attr["id"] = "radios_31";
    radios.AddCheckable(radio, "らじお　さん");

    //TextArea
    var textArea = new Sdx.Html.TextArea();
    textArea.Name = "textarea";
    textArea.AddValidator(new Sdx.Validation.NotEmpty());
    form.SetElement(textArea);

    if(Request.Form.Count > 0)
    {
      form.Bind(Request.Form);
      if (form.ExecValidators())
      {
        Sdx.Context.Current.Debug.Log("Is Valid !!");
      }

      Sdx.Context.Current.Debug.Log("Is Not Valid !!");
    }


    Sdx.Context.Current.Debug.Log(Request.Form, "POST");
    Sdx.Context.Current.Debug.Log(Request.QueryString, "GET");

    StreamReader reader = new StreamReader(Request.InputStream);
    Sdx.Context.Current.Debug.Log(reader.ReadToEnd(), "Request.InputStream");
  }
}