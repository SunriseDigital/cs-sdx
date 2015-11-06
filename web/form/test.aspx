<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test.aspx.cs" Inherits="form_test" %>
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
        <%= form["input_text"].Tag.Render(Attr.Create().AddClass("form-control")) %>
        <%= form["input_text"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["select"].Errors.Count > 0){ %> has-error<%}; %>">
        <%= form["select"].Tag.Render(Attr.Create().AddClass("form-control")) %>
        <%= form["select"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["check_list"].Errors.Count > 0){ %> has-error<%}; %>">
        <% form["check_list"].Tag.ForEach( checkbox => {%>
        <div class="checkbox">
          <%= checkbox.Render() %>
        </div>
        <%}); %>
        <%= form["check_list"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["radios"].Errors.Count > 0){ %> has-error<%}; %>">
        <% form["radios"].Tag.ForEach( radioLabel => {%>
        <% radioLabel.Children.First().Attr.AddClass("foo-bar"); %>
        <%= radioLabel.Render(Attr.Create().AddClass("radio-inline")) %>
        <%}); %>
        <%= form["radios"].Errors.Html.Render("h5", "text-danger", "list-unstyled") %>
      </div>
      <div class="form-group<%if (form["textarea"].Errors.Count > 0){ %> has-error<%}; %>">
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
