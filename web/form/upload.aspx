<%@ Page Language="C#" AutoEventWireup="true" CodeFile="upload.aspx.cs" Inherits="form_upload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
  <title></title>
  <script src="/sdx/package/jquery/2.2.3/jquery.js"></script>
  <script src="/sdx/package/jquery-file-upload/9.12.3/js/vendor/jquery.ui.widget.js"></script>
  <script src="/sdx/package/jquery-file-upload/9.12.3/js/jquery.iframe-transport.js"></script>
  <script src="/sdx/package/jquery-file-upload/9.12.3/js/jquery.fileupload.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <input id="fileupload" type="file" name="files" data-url="upload-point.aspx?foo=bar" multiple>
    <script>
      $(function () {
        $('#fileupload').fileupload({
          dataType: 'json',
          singleFileUploads: false,
          sequentialUploads: true
        }).bind("fileuploaddone", function (e, data) {
          $.each(data.result.files, function (index, file) {
            $('<img style="width: 100px;"/>').attr("src", file.name).appendTo(document.body);
          });
        }).bind("fileuploadfail", function (e, data) {
          try {
            var error = JSON.parse(data.jqXHR.responseText);
            if (error.type == "MaxRequestLength") {
              alert(error.maxLength + "KB以上はアップロードできません。");
            } else {
              throw "";
            }
          } catch (e) {
            alert("サーバーエラーです。")
          }
        })
      });
    </script>
    </div>
    </form>
</body>
</html>
