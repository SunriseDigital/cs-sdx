﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test.aspx.cs" Inherits="form_test" %>
<%@ Import Namespace="Sdx.Html" %>
<!DOCTYPE html>

<html>
<head>
  <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
  <title></title>
  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css" integrity="sha512-dTfge/zgoMYpP7QbHy4gWMEGsbsdZeCXz7irItjcC3sPUFtf0kuFbDz/ixG7ArTxmDjLXDmezHubeNikyKGVyQ==" crossorigin="anonymous">
</head>
<body>
  <div class="container">
    <h1>Form Test</h1>
    <form action="<%= HttpContext.Current.Request.RawUrl %>" method="post">
      <div class="form-group<%if (form["input_text"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>メールアドレス</label>
        <%= form["input_text"].Tag.Render(Attr.Create().AddClass("form-control")) %>
        <%= form["input_text"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["input_number"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>数字</label>
        <%= form["input_number"].Tag.Render(Attr.Create().AddClass("form-control")) %>
        <%= form["input_number"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["select"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>単一セレクト</label>
        <% form.As<Sdx.Html.Select>("select").Options.First().Text = "選択してください"; %>
        <%= form["select"].Tag.Render(Attr.Create().AddClass("form-control")) %>
        <%= form["select"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["multi_select"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>複数セレクト</label>
        <% form["multi_select"].Tag.Attr["size"] = "2"; %>
        <%= form["multi_select"].Tag.Render(Attr.Create().AddClass("form-control")) %>
        <%= form["multi_select"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["check_list"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>チェックリスト</label>
        <% form["check_list"].Tag.ForEach( checkbox => {%>
        <div class="checkbox">
          <%= checkbox.Render() %>
        </div>
        <%}); %>
        <%= form["check_list"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["radios"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>ラジオ</label>
        <div class="radio">
        <% form["radios"].Tag.ForEach( radioLabel => {%>
          <% radioLabel.Children.First().Attr.AddClass("foo-bar"); %>
          <%= radioLabel.Render(Attr.Create().AddClass("radio-inline")) %>
        <%}); %>
        </div>
        <%= form["radios"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["textarea"].Errors.Count > 0){ %> has-error<%}; %>">
        <label>長いテキスト</label>
        <%= form["textarea"].Tag.Render(Attr.Create().AddClass("form-control").Set("rows", "12")) %>
        <%= form["textarea"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["input_text"].Errors.Count > 0){ %> has-error<%}; %>">
        <input type="submit" name="submit" value="SAVE" >
      </div>
    </form>
  </div>
</body>
</html>
