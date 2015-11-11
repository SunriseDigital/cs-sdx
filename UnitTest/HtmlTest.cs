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
  public class HtmlTest : BaseTest
  {
    [Fact]
    public void TestAttr()
    {
      var attr = Sdx.Html.Attr.Create();

      attr.AddClass("foo");
      Assert.Equal("class=\"foo\"", attr.Render());

      attr.AddClass("bar", "hoge", "huga");
      Assert.Equal("class=\"foo bar hoge huga\"", attr.Render());

      attr.RemoveClass("hoge", "huga");

      attr.Set("data-attr", "datavalue");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());

      attr.SetStyle("width", "100px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px;\"", attr.Render());

      attr.SetStyle("height", "200px");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());

      attr.Set("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", attr.Render());

      //temp add

      Assert.Equal(
        "class=\"foo bar hoge\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"",
        attr.Merge(Sdx.Html.Attr.Create().AddClass("hoge", "bar")).Render()
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 150px; height: 200px; border: 1px;\" disabled=\"disabled\"",
        attr.Merge(Sdx.Html.Attr.Create().SetStyle("border", "1px").SetStyle("width", "150px")).Render()
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"update\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\" data-attr2=\"datavalue2\"",
        attr.Merge(Sdx.Html.Attr.Create().Set("data-attr", "update").Set("data-attr2", "datavalue2")).Render()
      );

      //Remove

      attr.Remove("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\"", attr.Render());

      attr.RemoveStyle("width");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"height: 200px;\"", attr.Render());

      attr.RemoveStyle("height");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\"", attr.Render());

      attr.RemoveClass("foo");
      Assert.Equal("class=\"bar\" data-attr=\"datavalue\"", attr.Render());

      attr.RemoveClass("bar");
      Assert.Equal("data-attr=\"datavalue\"", attr.Render());

      attr.Remove("data-attr");
      Assert.Equal("", attr.Render());
    }

    [Fact]
    public void TestHtml()
    {
      var html = new Sdx.Html.Tag("div");
      Assert.Equal("<div></div>", html.Render());

      Assert.Equal("<div class=\"foobar\"></div>", html.Render(Sdx.Html.Attr.Create().AddClass("foobar")));
      Assert.Equal("<div></div>", html.Render());

      html.Attr.AddClass("foo");
      Assert.Equal("<div class=\"foo\"></div>", html.Render());
      Assert.Equal("<div class=\"foo bar\"></div>", html.Render(Sdx.Html.Attr.Create().AddClass("bar")));
      Assert.Equal("<div class=\"foo\"></div>", html.Render());

      html.AddHtml(new Sdx.Html.RawText("bar"));
      Assert.Equal("<div class=\"foo\">bar</div>", html.Render());

      var br = new Sdx.Html.VoidTag("br");
      Assert.Equal("<br>", br.Render());

      var div = new Sdx.Html.Tag("div");
      var p = new Sdx.Html.Tag("p");
      div.AddHtml(p);

      p.AddHtml(new Sdx.Html.RawText("日本語"));
      p.AddHtml(new Sdx.Html.VoidTag("br"));
      p.AddHtml(new Sdx.Html.RawText("English"));

      var span = new Sdx.Html.Tag("span");
      span.AddHtml(new Sdx.Html.RawText("span"));
      span.Attr.AddClass("foo");
      p.AddHtml(span);
      Assert.Equal(
        "<div><p>日本語<br>English<span class=\"foo\">span</span></p></div>",
        div.Render()
      );

      span = new Sdx.Html.Tag("span");
      span
        .AddText("foobar")
        .AddText("日本語");

      Assert.Equal(
        "<span>foobar日本語</span>",
        span.Render()
      );

      span = new Sdx.Html.Tag("span");
      span.AddText("\"&<>");
      Assert.Equal(
        "<span>&quot;&amp;&lt;&gt;</span>",
        span.Render()
      );
    }

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
  <input type=""checkbox"" value=""1"" name=""checkboxies"" checked=""checked"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"">
</span>
"),
  group.Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"" checked=""checked"">
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
  <input type=""checkbox"" value=""2"" name=""checkboxies"" checked=""checked"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"" checked=""checked"">
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
<input type=""checkbox"" value=""2"" name=""checkboxies"" checked=""checked"">
"),
  group[1].Tag.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"" checked=""checked"">
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
  <option value=""1"" selected=""selected"">foo</option>
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
  <option value=""2"" selected=""selected"">bar</option>
</select>
"),
  select.Tag.Render()
);

      select.Multiple = true;
      select.Bind(new string[] { "1", "2" });
      Assert.Equal(2, select.Value.Count);
      Assert.Equal("1", select.Value[0]);
      Assert.Equal("2", select.Value[1]);
      Assert.Equal(HtmlLiner(@"
<select name=""select"" multiple=""multiple"">
  <option value=""1"" selected=""selected"">foo</option>
  <option value=""2"" selected=""selected"">bar</option>
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
    <option value=""2"" selected=""selected"">bar</option>
  </optgroup>
  <option value=""3"">zip</option>
</select>
"),
  select.Tag.Render()
);
      select.Multiple = true;
      select.Bind(new string[] { "2", "3" });
      Assert.Equal(2, select.Value.Count);
      Assert.Equal("2", select.Value[0]);
      Assert.Equal("3", select.Value[1]);
      Assert.Equal(HtmlLiner(@"
<select name=""select"" multiple=""multiple"">
  <optgroup label=""group1"">
    <option value=""1"">foo</option>
  </optgroup>
  <optgroup label=""group2"">
    <option value=""2"" selected=""selected"">bar</option>
  </optgroup>
  <option value=""3"" selected=""selected"">zip</option>
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
      Sdx.Context.Current.Lang = "ja";

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
      Assert.Equal("<ul class=\"sdx-has-error\"><li>必須項目です。</li><li>メールアドレスの書式が正しくありません。</li></ul>", loginId.Errors.Html.Render());
      Assert.Equal("必須項目です。", loginId.Errors[0].Message);
      Assert.Equal("ja", loginId.Errors[0].Lang);

      //BreakChain
      form = new Sdx.Html.Form();

      loginId = new Sdx.Html.InputText("login_id");
      loginId
        .AddValidator(new Sdx.Validation.NotEmpty(), true)
        .AddValidator(new Sdx.Validation.Email());
      form.SetElement(loginId);

      form.Bind(request.Form);

      Assert.False(form.ExecValidators());
      Assert.Equal(1, loginId.Errors.Count);
      Assert.Equal("<ul class=\"sdx-has-error\"><li>必須項目です。</li></ul>", loginId.Errors.Html.Render());

      mock.SetupGet(x => x.Form).Returns(new NameValueCollection {
        {"login_id", "some@mail.com" },
      });

      form.Bind(request.Form);

      Assert.True(form.ExecValidators());
      Assert.Equal(0, loginId.Errors.Count);
      Assert.Equal("", loginId.Errors.Html.Render());
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
  <option value=""10"" selected=""selected"">sigle10</option>
  <option value=""11"">sigle11</option>
</select>"), select.Tag.Render());

      Exception ex = Record.Exception(new Assert.ThrowsDelegate(() => {
        select.Bind(new string[] { "10", "11" });
      }));
      Assert.IsType<InvalidOperationException>(ex);

      select.Multiple = true;
      select.Bind(new string[] { "10", "11" });

      Assert.Equal(HtmlLiner(@"
<select name=""single_select"" multiple=""multiple"">
  <option value=""10"" selected=""selected"">sigle10</option>
  <option value=""11"" selected=""selected"">sigle11</option>
</select>"), select.Tag.Render());

      ex = Record.Exception(new Assert.ThrowsDelegate(() => {
        select.Bind("10");
      }));
      Assert.IsType<InvalidOperationException>(ex);

      Sdx.Context.Current.Debug.Log(select.Tag.Render());
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
