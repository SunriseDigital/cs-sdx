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
      Sdx.Context.Current.PreventDebugDisplay = true;

      var formName = Request.Params["name"];
      var response = new Dictionary<string, List<Dictionary<string, string>>>();
      response["files"] = new List<Dictionary<string, string>>();
      for (int i = 0; i < Request.Files.Count; i++)
			{
        if (formName != Request.Files.GetKey(i))
        {
          continue;
        }
        var uploaded = Request.Files.Get(i);
        var fileData = new Dictionary<string, string>();
        var path = Page.MapPath("~/tmp/" + uploaded.FileName);
        uploaded.SaveAs(path);
        fileData["name"] = Sdx.Util.Path.MapWebPath(path);
        fileData["size"] = uploaded.ContentLength.ToString();
        fileData["type"] = uploaded.ContentType;
        fileData["error"] = null;
        response["files"].Add(fileData);
			}

      Sdx.Web.Helper.JsonResponse(response);
    }
}