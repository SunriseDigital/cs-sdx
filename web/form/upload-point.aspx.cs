using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class form_upload_point : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      dynamic imageUploader = uploader;
      imageUploader.MinWidth = 3000;
      imageUploader.MinHeight = 3000;
      imageUploader.ScaleUp = true;
      imageUploader.UploadWebPath = "~/tmp/";
    }
}