<%@ Page Language="C#" AutoEventWireup="true" CodeFile="list.aspx.cs" Inherits="scaffold_area_list" %>
<%@ Register TagPrefix="Scaffold" TagName="list" Src="~\_sdx\control\scaffold\list.ascx" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <div>
      <Scaffold:list ID="list" runat="server" Name="area" />
    </div>
</body>
</html>
