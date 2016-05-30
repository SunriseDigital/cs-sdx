<%@ Page Language="C#" AutoEventWireup="true" CodeFile="upload.aspx.cs" Inherits="form_upload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
  <title></title>
  <!-- Latest compiled and minified CSS -->
<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css" integrity="sha384-1q8mTJOASx8j1Au+a5WDVnPi2lkFfwwEAa8hDDdjZlpLegxhjVME1fgjWPGmkzs7" crossorigin="anonymous">
  <link href="/sdx/package/jquery-file-upload/9.12.3/css/jquery.fileupload.css" rel="stylesheet" />
  <script src="/sdx/package/jquery/2.2.3/jquery.js"></script>
  <script src="/sdx/package/jquery-file-upload/9.12.3/js/vendor/jquery.ui.widget.js"></script>
  <script src="/sdx/package/jquery-file-upload/9.12.3/js/jquery.iframe-transport.js"></script>
  <script src="/sdx/package/jquery-file-upload/9.12.3/js/jquery.fileupload.js"></script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
      <span class="btn btn-success fileinput-button">
        画像をアップロード
        <input id="fileupload" type="file" name="files" data-url="upload-point.aspx" multiple>
      </span>
      <script>
        $(function () {
          $('#fileupload').fileupload({
            dataType: 'json',
            singleFileUploads: false,
            sequentialUploads: true,
            limitMultiFileUploadSize: 4096 * 1024
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
          }).bind('fileuploadprogress', function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
            //console.log('fileuploadprogress', progress);
          }).bind('fileuploadprogressall', function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
            //console.log('fileuploadprogressall', progress);
          })
        });
      </script>
    </div>
    </form>
</body>
</html>
