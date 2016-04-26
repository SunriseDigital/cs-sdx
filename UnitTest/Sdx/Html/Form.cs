using Xunit;
using UnitTest.DummyClasses;
using Moq;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Reflection;

#if ON_VISUAL_STUDIO
using FactAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestClassAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using ClassInitializeAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute;
using ClassCleanupAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;
#endif

using System;

namespace UnitTest
{
  [TestClass]
  public class Html_Form : BaseTest
  {
    [Fact]
    public void TestFormInputText()
    {
      var form = new Sdx.Html.Form();
      var loginId = new Sdx.Html.InputText();

      Assert.True(loginId.Value.IsEmpty);

      loginId.Name = "login_id";
      Assert.Equal("<input type=\"text\" name=\"login_id\">", loginId.Tag.Render());


      loginId.Bind("test_user");

      form.SetElement(loginId);
      Assert.Equal("<input type=\"text\" name=\"login_id\" value=\"test_user\">", form["login_id"].Tag.Render());

      Assert.Equal("test_user", loginId.Value.First());
      loginId.Bind("new_value");
      Assert.Equal("<input type=\"text\" name=\"login_id\" value=\"new_value\">", form["login_id"].Tag.Render());
      Assert.Equal("new_value", loginId.Value.First());
    }


    [Fact]
    public void TestFormTextArea()
    {
      var form = new Sdx.Html.Form();
      var comment = new Sdx.Html.TextArea();

      comment.Name = "comment";
      Assert.Equal("<textarea name=\"comment\"></textarea>", comment.Tag.Render());

      comment.Bind(@"日本語
改行もあったりする
English
");

      Assert.Equal(@"<textarea name=""comment"">日本語
改行もあったりする
English
</textarea>", comment.Tag.Render());

      Assert.Equal(@"日本語
改行もあったりする
English
", comment.Value.First());
    }


    [Fact]
    public void TestFormCheckBox()
    {
      var checkbox = new Sdx.Html.CheckBox();

      checkbox.Name = "checkbox";
      Assert.Equal("<input type=\"checkbox\" name=\"checkbox\">", checkbox.Tag.Render());

      checkbox.Tag.Attr["value"] = "chx_value";
      Assert.Equal("<input type=\"checkbox\" name=\"checkbox\" value=\"chx_value\">", checkbox.Tag.Render());
      //checkboxはValueに正しい値が代入されて初めてValueが取得可能になる。
      Assert.True(checkbox.Value.IsEmpty);
      checkbox.Bind("chx_value");
      Assert.Equal("chx_value", checkbox.Value.First());
    }

    [Fact]
    public void TestFormRadio()
    {
      var radio = new Sdx.Html.Radio();

      radio.Name = "radio";
      Assert.Equal("<input type=\"radio\" name=\"radio\">", radio.Tag.Render());

      radio.Tag.Attr["value"] = "chx_value";
      Assert.Equal("<input type=\"radio\" name=\"radio\" value=\"chx_value\">", radio.Tag.Render());
      //radioはValueに正しい値が代入されて初めてValueが取得可能になる。
      Assert.True(radio.Value.IsEmpty);
      radio.Bind("chx_value");
      Assert.Equal("chx_value", radio.Value.First());
    }

    [Fact]
    public void TestFormCheckableGroupCheckbox()
    {
      var group = new Sdx.Html.CheckableGroup();
      group.Name = "checkboxies";

      Sdx.Html.CheckBox checkbox;

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "1";
      group.AddCheckable(checkbox);

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "2";
      group.AddCheckable(checkbox);

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "3";
      group.AddCheckable(checkbox);

      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"">
</span>
"),
  group.Tag.Render()
      );

      Assert.Equal(0, group.Value.Count);

      group.Bind(new string[] { "1" });
      Assert.Equal(1, group.Value.Count);
      Assert.Equal("1", group.Value.First());
      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"" checked>
  <input type=""checkbox"" value=""2"" name=""checkboxies"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"">
</span>
"),
  group.Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"" checked>
"),
  group[0].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""2"" name=""checkboxies"">
"),
  group[1].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"">
"),
  group[2].Tag.Render()
);

      group.Bind(new string[] { "2", "3" });
      Assert.Equal(2, group.Value.Count);
      Assert.Equal("2", group.Value[0]);
      Assert.Equal("3", group.Value[1]);
      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"" checked>
  <input type=""checkbox"" value=""3"" name=""checkboxies"" checked>
</span>
"),
  group.Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"">
"),
  group[0].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""2"" name=""checkboxies"" checked>
"),
  group[1].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"" checked>
"),
  group[2].Tag.Render()
);

      group.Bind(new string[0]);
      Assert.Equal(0, group.Value.Count);
      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"">
</span>
"),
  group.Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"">
"),
  group[0].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""2"" name=""checkboxies"">
"),
  group[1].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"">
"),
  group[2].Tag.Render()
);
    }

    [Fact]
    public void TestFormElementGroupCheckboxWithLabel()
    {
      var group = new Sdx.Html.CheckableGroup();
      group.Name = "checkboxies";

      Sdx.Html.CheckBox checkbox;

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "1";
      group.AddCheckable(checkbox, "foo");

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "2";
      group.AddCheckable(checkbox, "bar");

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "3";
      group.AddCheckable(checkbox, "zip");

      Assert.Equal(HtmlLiner(@"
<span>
  <label><input type=""checkbox"" value=""1"" name=""checkboxies"">foo</label>
  <label><input type=""checkbox"" value=""2"" name=""checkboxies"">bar</label>
  <label><input type=""checkbox"" value=""3"" name=""checkboxies"">zip</label>
</span>
"),
  group.Tag.Render()
      );

      group = new Sdx.Html.CheckableGroup();
      group.Name = "checkboxies";

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "1";
      checkbox.Tag.Attr["id"] = "check_foo";
      group.AddCheckable(checkbox, "foo");

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "2";
      checkbox.Tag.Attr["id"] = "check_bar";
      group.AddCheckable(checkbox, "bar");

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Tag.Attr["value"] = "3";
      checkbox.Tag.Attr["id"] = "check_zip";
      group.AddCheckable(checkbox, "zip");

      Assert.Equal(HtmlLiner(@"
<span>
  <label for=""check_foo""><input type=""checkbox"" value=""1"" id=""check_foo"" name=""checkboxies"">foo</label>
  <label for=""check_bar""><input type=""checkbox"" value=""2"" id=""check_bar"" name=""checkboxies"">bar</label>
  <label for=""check_zip""><input type=""checkbox"" value=""3"" id=""check_zip"" name=""checkboxies"">zip</label>
</span>
"),
  group.Tag.Render()
      );
    }

    [Fact]
    public void TestFormSelect()
    {
      var select = new Sdx.Html.Select();
      select.Name = "select";

      Assert.Equal("<select name=\"select\"></select>", select.Tag.Render());

      Sdx.Html.Option option;

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "1";
      option.Text = "foo";
      select.AddOption(option);

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "2";
      option.Text = "bar";
      select.AddOption(option);

      Assert.Equal(HtmlLiner(@"
<select name=""select"">
  <option value=""1"">foo</option>
  <option value=""2"">bar</option>
</select>
"),
  select.Tag.Render()
);

      Assert.Equal(0, select.Value.Count);

      select.Bind("1");
      Assert.Equal(1, select.Value.Count);
      Assert.Equal("1", select.Value.First());
      Assert.Equal(HtmlLiner(@"
<select name=""select"">
  <option value=""1"" selected>foo</option>
  <option value=""2"">bar</option>
</select>
"),
  select.Tag.Render()
);

      select.Bind("2");
      Assert.Equal(1, select.Value.Count);
      Assert.Equal("2", select.Value.First());
      Assert.Equal(HtmlLiner(@"
<select name=""select"">
  <option value=""1"">foo</option>
  <option value=""2"" selected>bar</option>
</select>
"),
  select.Tag.Render()
);

      select.IsMultiple = true;
      select.Bind(new string[] { "1", "2" });
      Assert.Equal(2, select.Value.Count);
      Assert.Equal("1", select.Value[0]);
      Assert.Equal("2", select.Value[1]);
      Assert.Equal(HtmlLiner(@"
<select name=""select"" multiple>
  <option value=""1"" selected>foo</option>
  <option value=""2"" selected>bar</option>
</select>
"),
  select.Tag.Render()
);
    }

    [Fact]
    public void TestFormSelectOptgroup()
    {
      var select = new Sdx.Html.Select();
      select.Name = "select";
      Sdx.Html.Option option;

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "1";
      option.Text = "foo";
      select.AddOption(option, "group1");

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "2";
      option.Text = "bar";
      select.AddOption(option, "group2");

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "3";
      option.Text = "zip";
      select.AddOption(option);

      Assert.Equal(HtmlLiner(@"
<select name=""select"">
  <optgroup label=""group1"">
    <option value=""1"">foo</option>
  </optgroup>
  <optgroup label=""group2"">
    <option value=""2"">bar</option>
  </optgroup>
  <option value=""3"">zip</option>
</select>
"),
  select.Tag.Render()
);

      Assert.Equal(0, select.Value.Count);

      select.Bind("2");
      Assert.Equal(1, select.Value.Count);
      Assert.Equal("2", select.Value.First());
      Assert.Equal(HtmlLiner(@"
<select name=""select"">
  <optgroup label=""group1"">
    <option value=""1"">foo</option>
  </optgroup>
  <optgroup label=""group2"">
    <option value=""2"" selected>bar</option>
  </optgroup>
  <option value=""3"">zip</option>
</select>
"),
  select.Tag.Render()
);
      select.IsMultiple = true;
      select.Bind(new string[] { "2", "3" });
      Assert.Equal(2, select.Value.Count);
      Assert.Equal("2", select.Value[0]);
      Assert.Equal("3", select.Value[1]);
      Assert.Equal(HtmlLiner(@"
<select name=""select"" multiple>
  <optgroup label=""group1"">
    <option value=""1"">foo</option>
  </optgroup>
  <optgroup label=""group2"">
    <option value=""2"" selected>bar</option>
  </optgroup>
  <option value=""3"" selected>zip</option>
</select>
"),
  select.Tag.Render()
);
    }

    [Fact]
    public void TestFormBind()
    {
      var mock = new Mock<System.Web.HttpRequestBase>();
      mock.SetupGet(x => x.Form).Returns(new NameValueCollection {
        {"login_id", "foo@bar.com" },
      });

      var form = new Sdx.Html.Form();
      var loginId = new Sdx.Html.InputText("login_id");

      form.SetElement(loginId);

      var request = mock.Object;

      form.Bind(request.Form);

      Assert.Equal("<input type=\"text\" name=\"login_id\" value=\"foo@bar.com\">", loginId.Tag.Render());
      Sdx.Context.Current.Debug.Log(loginId.Tag.Render());
    }

    [Fact]
    public void TestFormValidation()
    {
      Sdx.Context.Current.Culture = new CultureInfo("ja-JP");

      var mock = new Mock<System.Web.HttpRequestBase>();
      mock.SetupGet(x => x.Form).Returns(new NameValueCollection {
        {"login_id", "" },
      });

      var request = mock.Object;

      var form = new Sdx.Html.Form();

      var loginId = new Sdx.Html.InputText("login_id");
      loginId
        .AddValidator(new Sdx.Validation.NotEmpty())
        .AddValidator(new Sdx.Validation.Email());
      form.SetElement(loginId);

      form.Bind(request.Form);

      Assert.False(form.ExecValidators());
      Assert.Equal(2, loginId.Errors.Count);
      Assert.Equal("<ul class=\"sdx-has-error\"><li>必須項目です。</li><li>メールアドレスの書式が正しくありません。</li></ul>", loginId.Errors.Html().Render());
      Assert.Equal("必須項目です。", loginId.Errors[0].Message);

      //BreakChain
      form = new Sdx.Html.Form();

      loginId = new Sdx.Html.InputText("login_id");
      loginId
        .AddValidator(new Sdx.Validation.NotEmpty(), true)
        .AddValidator(new Sdx.Validation.Email());
      form.SetElement(loginId);

      Assert.Equal(0, loginId.Errors.Count);

      form.Bind(request.Form);

      Assert.False(form.ExecValidators());
      Assert.Equal(1, loginId.Errors.Count);
      Assert.Equal("<ul class=\"sdx-has-error\"><li>必須項目です。</li></ul>", loginId.Errors.Html().Render());

      mock.SetupGet(x => x.Form).Returns(new NameValueCollection {
        {"login_id", "some@mail.com" },
      });

      form.Bind(request.Form);

      Assert.True(form.ExecValidators());
      Assert.Equal(0, loginId.Errors.Count);
      Assert.Equal("", loginId.Errors.Html().Render());
    }

    [Fact]
    public void TestFormSelectMultiple()
    {

      Sdx.Html.Option option;

      var select = new Sdx.Html.Select("single_select");

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "10";
      option.Text = "sigle10";
      select.AddOption(option);

      option = new Sdx.Html.Option();
      option.Tag.Attr["value"] = "11";
      option.Text = "sigle11";
      select.AddOption(option);

      select.Bind("10");

      Assert.Equal(HtmlLiner(@"
<select name=""single_select"">
  <option value=""10"" selected>sigle10</option>
  <option value=""11"">sigle11</option>
</select>"), select.Tag.Render());

      Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
        select.Bind(new string[] { "10", "11" });
      }));
      Assert.IsType<InvalidOperationException>(ex);

      select.IsMultiple = true;
      select.Bind(new string[] { "10", "11" });

      Assert.Equal(HtmlLiner(@"
<select name=""single_select"" multiple>
  <option value=""10"" selected>sigle10</option>
  <option value=""11"" selected>sigle11</option>
</select>"), select.Tag.Render());

      ex = Record.Exception(new Assert.ThrowsDelegate(() => {
        select.Bind("10");
      }));
      Assert.IsType<InvalidOperationException>(ex);

      Sdx.Context.Current.Debug.Log(select.Tag.Render());
    }

    [Fact]
    public void TestFormInputHidden()
    {
      var radio = new Sdx.Html.InputHidden();

      radio.Name = "hidden";
      Assert.Equal("<input type=\"hidden\" name=\"hidden\">", radio.Tag.Render());
      radio.Bind("hidden_value");
      Assert.Equal("hidden_value", radio.Value.First());
      Assert.Equal("<input type=\"hidden\" name=\"hidden\" value=\"hidden_value\">", radio.Tag.Render());
    }

    [Fact]
    public void TestFormRequireValidate()
    {
      var input = new Sdx.Html.InputText("test");

      input.AddValidator(new Sdx.Validation.Numeric());
      input.IsAllowEmpty = true;
      input.Bind("");
      input.ExecValidators();
      Assert.Equal(0, input.Errors.Count);

      input.IsAllowEmpty = false;
      input.ExecValidators();
      Assert.Equal(1, input.Errors.Count);
    }

    [Fact]
    public void TestIsSecret()
    {
      ((Action)(() =>
      {
        var input = new Sdx.Html.InputText("secret");
        input.AddValidator(new Sdx.Validation.NotEmpty());
        input.IsSecret = true;

        input.Bind("");

        Assert.Equal("<input type=\"text\" name=\"secret\">", input.Tag.Render());
        Assert.True(input.Value.IsEmpty);

        //ここからサブミットされたときの挙動
        input.Bind("");
        Assert.Equal("<input type=\"text\" name=\"secret\">", input.Tag.Render());
        //最初のBindが空だったら新規登録なので2度目のバリデータは動かす
        Assert.False(input.ExecValidators());
      }))();

      ((Action)(() => 
      {
        var input = new Sdx.Html.InputText("secret");
        input
          .AddValidator(new Sdx.Validation.NotEmpty())
          .AddValidator(new Sdx.Validation.StringLength(min: 5));
        input.IsSecret = true;

        input.Bind("100");

        //BindしてもvalueにはRenderされません
        Assert.Equal("<input type=\"text\" name=\"secret\">", input.Tag.Render());

        //ここからサブミットされたときの挙動
        input.Bind("");
        Assert.Equal("<input type=\"text\" name=\"secret\" value=\"\">", input.Tag.Render());
        //必須項目でも2度目で空は未編集とみなし、テストを通す。
        Assert.True(input.ExecValidators());

        input.Bind("1234");
        Assert.Equal("<input type=\"text\" name=\"secret\" value=\"1234\">", input.Tag.Render());
        //一度目が空ではなく、2度目も空じゃないときはバリデータを動かす
        Assert.False(input.ExecValidators());
      }))();
    }

    [Fact]
    public void TestFormToNameValueCollection()
    {
      var form = new Sdx.Html.Form();

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

      var existingData = new NameValueCollection();
      existingData.Add("input_text",   "");
      existingData.Add("input_number", "");
      existingData.Add("start_date ",  "");
      existingData.Add("select",       "");
      existingData.Add("multi_select", "");
      existingData.Add("check_list",   "");
      existingData.Add("radios",       "");
      existingData.Add("textarea",     "");
      existingData.Add("secret",       "OriginValue");

      form.Bind(existingData);

      var post = new NameValueCollection();
      post.Add("input_text", "test@example.com");
      post.Add("input_number","123");
      post.Add("start_date", "2015-10-02");
      post.Add("select", "10");
      post.Add("multi_select", "100");
      post.Add("multi_select", "101");
      post.Add("check_list", "20");
      post.Add("check_list", "21");
      post.Add("radios", "31");
      post.Add("textarea", "aaaaaaaaaaaaaaaaaa");
      post.Add("secret", "");//secretは空の時更新をしない

      //validate前
      Exception ex = Record.Exception(new Assert.ThrowsDelegate(() =>
      {
        var errorResult = form.ToNameValueCollection();
      }));

      Assert.IsType<InvalidOperationException>(ex);

      form.Bind(post);
      Assert.True(form.ExecValidators());

      var result = form.ToNameValueCollection();

      Assert.Equal("test@example.com", result["input_text"]);
      Assert.Equal("123", result["input_number"]);
      Assert.Equal("2015-10-02", result["start_date"]);
      Assert.Equal("10", result["select"]);
      Assert.Equal("100,101", result["multi_select"]);
      Assert.Equal("20,21", result["check_list"]);
      Assert.Equal("31", result["radios"]);
      Assert.Equal("aaaaaaaaaaaaaaaaaa", result["textarea"]);
      Assert.Null(result["secret"]);//secretは空の時更新をしない

      //secret更新
      post.Set("secret", "NewValue");
      form.Bind(post);
      result = form.ToNameValueCollection();
      Assert.Equal("NewValue", result["secret"]);
    }

    //[Fact]
    //public void TestMock()
    //{
    //  var mock = new Mock<System.Web.HttpRequestBase>();
    //  mock.SetupGet(x => x.Form).Returns(new NameValueCollection {
    //    {"name1", "post1" },
    //  });

    //  mock.SetupGet(x => x.QueryString).Returns(new NameValueCollection {
    //    {"name1", "get1" },
    //    {"name2", "get2" },
    //  });

    //  var request = mock.Object;

    //  Sdx.Context.Current.Debug.Log(request.QueryString);
    //}
  }
}
