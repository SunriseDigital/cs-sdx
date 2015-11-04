using Xunit;
using UnitTest.DummyClasses;
using Moq;
using System.Collections.Specialized;
using System.Web;
using System.IO;

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

      attr.Add("disabled");
      Assert.Equal("class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"", attr.Render());

      //temp add

      Assert.Equal(
        "class=\"foo bar hoge\" data-attr=\"datavalue\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\"",
        attr.Render(Sdx.Html.Attr.Create().AddClass("hoge", "bar"))
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"datavalue\" style=\"width: 150px; height: 200px; border: 1px;\" disabled=\"disabled\"",
        attr.Render(Sdx.Html.Attr.Create().SetStyle("border", "1px").SetStyle("width", "150px"))
      );

      Assert.Equal(
        "class=\"foo bar\" data-attr=\"update\" style=\"width: 100px; height: 200px;\" disabled=\"disabled\" data-attr2=\"datavalue2\"",
        attr.Render(Sdx.Html.Attr.Create().Set("data-attr", "update").Set("data-attr2", "datavalue2"))
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

      html.Attr.AddClass("foo");
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
    }

    [Fact]
    public void TestForm()
    {
      var form = new Sdx.Html.Form();
      Assert.Equal("<form method=\"post\"></form>", form.Render());

      form.Action = "/foo/bar";
      Assert.Equal("<form method=\"post\" action=\"/foo/bar\"></form>", form.Render());

      form.Method = Sdx.Html.Form.MethodType.Get;
      Assert.Equal("<form method=\"get\" action=\"/foo/bar\"></form>", form.Render());

      HttpContext.Current = new HttpContext(
        new HttpRequest("", "http://test.cs-sdx.com/form/current", "foo=bar"),
        new HttpResponse(new StringWriter())
      );
      form.SetActionToCurrent();
      Assert.Equal("<form method=\"get\" action=\"/form/current?foo=bar\"></form>", form.Render());
    }

    [Fact]
    public void TestFormInputText()
    {
      var form = new Sdx.Html.Form();
      var loginId = new Sdx.Html.InputText();

      Assert.Equal("", loginId.Value.First);

      loginId.Name = "login_id";
      Assert.Equal("<input type=\"text\" value=\"\" name=\"login_id\">", loginId.Render());

      form.SetElement(loginId);
      Assert.Equal("<form method=\"post\"><input type=\"text\" value=\"\" name=\"login_id\"></form>", form.Render());
      loginId.Bind("test_user");

      form.SetElement(loginId);
      Assert.Equal("<form method=\"post\"><input type=\"text\" value=\"test_user\" name=\"login_id\"></form>", form.Render());
      Assert.Equal("<input type=\"text\" value=\"test_user\" name=\"login_id\">", form["login_id"].Render());

      Assert.Equal("test_user", loginId.Value.First);
      loginId.Bind("new_value");
      Assert.Equal("<input type=\"text\" value=\"new_value\" name=\"login_id\">", form["login_id"].Render());
      Assert.Equal("new_value", loginId.Value.First);
    }


    [Fact]
    public void TestFormTextArea()
    {
      var form = new Sdx.Html.Form();
      var comment = new Sdx.Html.TextArea();

      comment.Name = "comment";
      Assert.Equal("<textarea name=\"comment\"></textarea>", comment.Render());

      comment.Bind(@"日本語
改行もあったりする
English
");

      Assert.Equal(@"<textarea name=""comment"">日本語
改行もあったりする
English
</textarea>", comment.Render());

      Assert.Equal(@"日本語
改行もあったりする
English
", comment.Value.First);
    }


    [Fact]
    public void TestFormCheckBox()
    {
      var checkbox = new Sdx.Html.CheckBox();

      checkbox.Name = "checkbox";
      Assert.Equal("<input type=\"checkbox\" value=\"\" name=\"checkbox\">", checkbox.Render());

      checkbox.Attr["value"] = "chx_value";
      Assert.Equal("<input type=\"checkbox\" value=\"chx_value\" name=\"checkbox\">", checkbox.Render());
      //checkboxはValueに正しい値が代入されて初めてValueが取得可能になる。
      Assert.Equal("", checkbox.Value.First);
      checkbox.Bind("chx_value");
      Assert.Equal("chx_value", checkbox.Value.First);
    }

    [Fact]
    public void TestFormElementGroupCheckbox()
    {
      var group = new Sdx.Html.ElementGroup();
      group.Name = "checkboxies";

      Sdx.Html.CheckBox checkbox;

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Attr["value"] = "1";
      group.AddElement(checkbox);

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Attr["value"] = "2";
      group.AddElement(checkbox);

      checkbox = new Sdx.Html.CheckBox();
      checkbox.Attr["value"] = "3";
      group.AddElement(checkbox);

      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"">
</span>
"),
  group.Render()
      );

      Assert.Equal(0, group.Value.Count);

      group.Bind(new string[] { "1" });
      Assert.Equal(1, group.Value.Count);
      Assert.Equal("1", group.Value.First);
      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"" checked=""checked"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"">
</span>
"),
  group.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"" checked=""checked"">
"),
  group[0].Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""2"" name=""checkboxies"">
"),
  group[1].Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"">
"),
  group[2].Render()
);

      group.Bind(new string[] { "2", "3" });
      Assert.Equal(2, group.Value.Count);
      Assert.Equal("2", group.Value.All[0]);
      Assert.Equal("3", group.Value.All[1]);
      Assert.Equal(HtmlLiner(@"
<span>
  <input type=""checkbox"" value=""1"" name=""checkboxies"">
  <input type=""checkbox"" value=""2"" name=""checkboxies"" checked=""checked"">
  <input type=""checkbox"" value=""3"" name=""checkboxies"" checked=""checked"">
</span>
"),
  group.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"">
"),
  group[0].Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""2"" name=""checkboxies"" checked=""checked"">
"),
  group[1].Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"" checked=""checked"">
"),
  group[2].Render()
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
  group.Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""1"" name=""checkboxies"">
"),
  group[0].Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""2"" name=""checkboxies"">
"),
  group[1].Render()
);

      Assert.Equal(HtmlLiner(@"
<input type=""checkbox"" value=""3"" name=""checkboxies"">
"),
  group[2].Render()
);
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
