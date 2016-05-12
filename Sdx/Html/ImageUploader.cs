using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class ImageUploader : FormElement
  {
    private Html.Tag listHtml;
    private Html.Tag buttonLabel;
    private Html.VoidTag inputFile;

    public ImageUploader():base()
    {
      MaxCount = 1;
    }

    public ImageUploader(string name)
      : base(name)
    {

    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(true);
    }

    protected internal override Tag CreateTag()
    {
      var wrapper = new Sdx.Html.Tag("div");
      wrapper.Attr.AddClass("sdx-image-uploader");

      var button = new Sdx.Html.Tag("span");
      wrapper.AddHtml(button);
      button.Attr.AddClass("fileinput-button", "btn");

      buttonLabel = new Sdx.Html.Tag("span");
      button.AddHtml(buttonLabel);
      buttonLabel.Attr.AddClass("btn-label");

      inputFile = new Sdx.Html.VoidTag("input");
      button.AddHtml(inputFile);
      inputFile.Attr.AddClass("input");
      inputFile.Attr["type"] = "file";

      listHtml = new Sdx.Html.Tag("ul");
      wrapper.AddHtml(listHtml);
      listHtml.Attr.AddClass("list-inline", "images", "clearfix");

      return wrapper;
    }

    protected internal override void BindValueToTag()
    {
      foreach (var val in Value)
      {
        if(val == "")
        {
          continue;
        }
        var hidden = new Sdx.Html.InputHidden();
        hidden.Tag.Attr["value"] = val;
        hidden.Tag.Attr.AddClass("server-images");
        Tag.AddHtml(hidden.Tag);
      }
    }

    public HtmlBase ButtonLabel
    {
      set
      {
        buttonLabel.children.RemoveRange(0, buttonLabel.children.Count);
        buttonLabel.AddChild(value);
      }

      get
      {
        return buttonLabel.children[0];
      }
    }

    public string UploadPath
    {
      set
      {
        inputFile.Attr["data-url"] = value;
      }

      get
      {
        return inputFile.Attr["data-url"];
      }
    }

    public override string Name
    {
      get
      {
        return inputFile.Attr["data-submit-name"];
      }
      set
      {
        inputFile.Attr["data-submit-name"] = value;
        inputFile.Attr["name"] = value + "--file-input";
      }
    }

    public int MaxCount
    {
      get 
      {
        return Int32.Parse(inputFile.Attr["data-max-count"]);
      }

      set 
      {
        inputFile.Attr["data-max-count"] = value.ToString();
        if(value > 1)
        {
          inputFile.Attr.Set("multiple");
        }
        else
        {
          inputFile.Attr.Remove("multiple");
        }
      }
    }

    public int ThumbWidth
    {
      get
      {
        return Int32.Parse(inputFile.Attr["data-thumb-width"]);
      }
      set
      {
        inputFile.Attr["data-thumb-width"] = value.ToString();
      }
    }

    public string DeleteLabel
    {
      set
      {
        inputFile.Attr["data-delete-label"] = value;
      }

      get
      {
        return inputFile.Attr["data-delete-label"];
      }
    }
  }
}
