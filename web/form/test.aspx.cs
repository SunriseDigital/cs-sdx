﻿using System;
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
    inputText
      .AddValidator(new Sdx.Validation.NotEmpty(), true)
      .AddValidator(new Sdx.Validation.Email());
    form.SetElement(inputText);

    var inputNumber = new Sdx.Html.InputText();
    inputNumber.Name = "input_number";
    inputNumber
      .AddValidator(new Sdx.Validation.NotEmpty(), true)
      .AddValidator(new Sdx.Validation.GreaterThan(100))
      .AddValidator(new Sdx.Validation.LessThan(200));
    form.SetElement(inputNumber);

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

    //select multi
    var multiSelect = new Sdx.Html.Select();
    multiSelect.Name = "multi_select";
    multiSelect.IsMultiple = true;
    multiSelect.AddValidator(new Sdx.Validation.NotEmpty());
    form.SetElement(multiSelect);

    option = new Sdx.Html.Option();
    option.Tag.Attr["value"] = "100";
    option.Text = "選択肢１００";
    multiSelect.AddOption(option);

    option = new Sdx.Html.Option();
    option.Tag.Attr["value"] = "101";
    option.Text = "選択肢１０１";
    multiSelect.AddOption(option);


    //Checkbox
    var checkList = new Sdx.Html.CheckBoxGroup();
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
    var radios = new Sdx.Html.RadioGroup();
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
    textArea
      .AddValidator(new Sdx.Validation.NotEmpty())
      .AddValidator(new Sdx.Validation.StringLength(10, 30));
    form.SetElement(textArea);

    //date
    var startDate = new Sdx.Html.InputDate("start_date");
    startDate.IsAllowEmpty = true;
    startDate
      .AddValidator(new Sdx.Validation.DateSpan(new DateTime(2015, 10, 1), new DateTime(2015, 10, 31), "yyyy年M月d日"))
      ;
    form.SetElement(startDate);

    //Secret
    Sdx.Html.FormElement secret = new Sdx.Html.InputText();
    secret.Name = "secret";
    secret.IsSecret = true;
    secret
      .AddValidator(new Sdx.Validation.NotEmpty())
      .AddValidator(new Sdx.Validation.StringLength(min: 5));
    form.SetElement(secret);
    secret.Bind("CurrentValue");

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