<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test.aspx.cs" Inherits="form_test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
  <title></title>
  <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css" integrity="sha512-dTfge/zgoMYpP7QbHy4gWMEGsbsdZeCXz7irItjcC3sPUFtf0kuFbDz/ixG7ArTxmDjLXDmezHubeNikyKGVyQ==" crossorigin="anonymous">
</head>
<body>
	<div class="container">
   <form action="/form/test.aspx" method="post" enctype="application/x-www-form-urlencoded">
    <div class="form-group">
      <input type="type" name="name" class="form-control" value="" />
    </div>

    <div class="form-group">
      <input type="type" name="name" class="form-control" value="" />
    </div>

    <div class="form-group">
      <input type="type" name="single" class="form-control" value="" />
    </div>

    <div class="form-group">
      <input type="file" name="file" class="form-control" />
    </div>

    <div class="checkbox">
      <label>
        <input type="checkbox" name="checkbox" value="checkbox1"> Checkbox 1
      </label>
      <label>
       <input type="checkbox" name="checkbox" value="checkbox2"> Checkbox 2
      </label>
    </div>

    <div class="form-group">
      <input type="submit" name="submit" class="form-control" value="submit" />
    </div>
  </form>
</div>
</body>
</html>
