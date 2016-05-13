using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class ImageUploader : FormElement
  {
    private Html.Tag wrapper;
    private Html.Tag listHtml;
    private Html.Tag buttonLabel;
    private Html.VoidTag inputFile;

    public ImageUploader():base()
    {
      this.InitDefaultValues();    
    }

    public ImageUploader(string name)
      : base(name)
    {
      this.InitDefaultValues();
    }

    private void InitDefaultValues()
    {
      MaxCount = 1;
      MaxCountMessage = Sdx.I18n.GetString("%MaxCount%までアップロード可能です。以下の画像はアップロードされませんでした。");
    }

    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(true);
    }

    protected internal override Tag CreateTag()
    {
      wrapper = new Sdx.Html.Tag("div", "sdx-image-uploader");

      var button = new Sdx.Html.Tag("span", "fileinput-button", "btn");
      wrapper.AddHtml(button);

      buttonLabel = new Sdx.Html.Tag("span", "btn-label");
      button.AddHtml(buttonLabel);

      inputFile = new Sdx.Html.VoidTag("input", "input");
      button.AddHtml(inputFile);
      inputFile.Attr["type"] = "file";

      var progress = new Sdx.Html.Tag("div", "progress");
      wrapper.AddHtml(progress);
      var progressBar = new Sdx.Html.Tag("div", attr => {
        attr.AddClass("progress-bar");
        attr.SetStyle("width", "0%");
      });
      progress.AddHtml(progressBar);

      listHtml = new Sdx.Html.Tag("ul", "list-inline", "images", "clearfix");
      wrapper.AddHtml(listHtml);

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
        return wrapper.Attr["data-submit-name"];
      }
      set
      {
        wrapper.Attr["data-submit-name"] = value;
        inputFile.Attr["name"] = value + "--file-input";
      }
    }

    public int MaxCount
    {
      get 
      {
        return Int32.Parse(wrapper.Attr["data-max-count"]);
      }

      set 
      {
        wrapper.Attr["data-max-count"] = value.ToString();
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
        return Int32.Parse(wrapper.Attr["data-thumb-width"]);
      }
      set
      {
        wrapper.Attr["data-thumb-width"] = value.ToString();
      }
    }

    public string DeleteLabel
    {
      set
      {
        wrapper.Attr["data-delete-label"] = value;
      }

      get
      {
        return wrapper.Attr["data-delete-label"];
      }
    }

    public string MaxCountMessage
    {
      set
      {
        wrapper.Attr["data-max-count-message"] = value;
      }

      get
      {
        return wrapper.Attr["data-max-count-message"];
      }
    }
  }
}
