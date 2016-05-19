<%@ Page Language="C#" AutoEventWireup="true" CodeFile="list.aspx.cs" Inherits="scaffold_area_list" %>
<%@ Register TagPrefix="Scaffold" TagName="list" Src="~\sdx\_private\cs\scaffold\list.ascx" %>
<%@ Register TagPrefix="Scaffold" TagName="head" Src="~\sdx\_private\cs\scaffold\head.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <Scaffold:head ID="head" runat="server" />
</head>
<body>
    <div class="container">
      <Scaffold:list ID="list" runat="server" />
    </div>
</body>
</html>
