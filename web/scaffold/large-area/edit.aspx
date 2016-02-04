<%@ Page Language="C#" AutoEventWireup="true" CodeFile="edit.aspx.cs" Inherits="scaffold_area_edit" %>
<%@ Register TagPrefix="Scaffold" TagName="list" Src="~\lib\_private\control\scaffold\edit.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css">
</head>
<body>
  <div class="container">
    <Scaffold:list ID="list" runat="server" Name="large_area" />
  </div>
</body>
</html>
