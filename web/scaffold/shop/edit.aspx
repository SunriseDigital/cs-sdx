<%@ Page Language="C#" AutoEventWireup="true" CodeFile="edit.aspx.cs" Inherits="scaffold_area_edit" %>
<%@ Register TagPrefix="Scaffold" TagName="edit" Src="~\sdx\_private\cs\scaffold\edit.ascx" %>
<%@ Register TagPrefix="Scaffold" TagName="head" Src="~\sdx\_private\cs\scaffold\head.ascx" %>
<!DOCTYPE html>

<html>
  <head>
    <meta charset="utf-8">
    <title></title>
    <Scaffold:head ID="head" runat="server" />
  </head>
  <body>
    <div class="container">
      <Scaffold:edit ID="edit" runat="server" OutlineRank="3" />
    </div>
  </body>
</html>
