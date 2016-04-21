﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="edit.aspx.cs" Inherits="scaffold_area_edit" %>
<%@ Register TagPrefix="Scaffold" TagName="list" Src="~\sdx\_private\csharp\scaffold\edit.ascx" %>
<%@ Register TagPrefix="Scaffold" TagName="head" Src="~\sdx\_private\csharp\scaffold\head.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <Scaffold:head ID="head" runat="server" />
</head>
<body>
  <div class="container">
    <Scaffold:list ID="list" runat="server" TitleTag="h3" />
  </div>
</body>
</html>
