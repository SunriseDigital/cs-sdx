<%@ Page Language="C#" AutoEventWireup="true" CodeFile="list.aspx.cs" Inherits="scaffold_shop_list" %>
<%@ Register TagPrefix="Scaffold" TagName="list" Src="~\sdx\_private\cs\scaffold\list.ascx" %>
<%@ Register TagPrefix="Scaffold" TagName="head" Src="~\sdx\_private\cs\scaffold\head.ascx" %>
<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8">
    <title></title>
    <Scaffold:head ID="head1" runat="server" />
  </head>
  <body>
    <div class="container">
      <Scaffold:list ID="list" runat="server" OutlineRank="3" />
    </div>
  </body>
</html>
