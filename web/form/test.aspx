<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test.aspx.cs" Inherits="form_test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <div>
    cs-sdx test index
    </div>
    <form action="/form/test.aspx" method="post" enctype="application/x-www-form-urlencoded">
        <input type="type" name="name" value="" />
        <input type="type" name="name" value="" />
        <input type="type" name="single" value="" />
        <input type="file" name="file" />
        <input type="submit" name="submit" value="submit" />
    </form>
</body>
</html>
