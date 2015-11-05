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
        <%= form.RenderStartTag() %> 
            <div class="form-group">
                <%= form["input_text"].Tag.Render(Attr.Create().AddClass("form-control")) %>
            </div>
            <div class="form-group">
                <%= form["select"].Tag.Render(Attr.Create().AddClass("form-control")) %>
            </div>
            <div class="form-group">
                <% form["check_list"].Tag.ForEach( checkbox => {%>
                <div class="checkbox">
                    <%= checkbox.Render() %>
                </div>
                <%}); %>
            </div>
            <div class="form-group">
                <% form["radios"].Tag.ForEach( radio => {%>
                <%= radio.Render(Attr.Create().AddClass("radio-inline")) %>
                <%}); %>
            </div>
            <div class="form-group">
                <%= form["textarea"].Tag.Render(Attr.Create().AddClass("form-control").Set("rows", "12")) %>
            </div>
        <%= form.RenderEndTag() %>
    </div>
</body>
</html>
