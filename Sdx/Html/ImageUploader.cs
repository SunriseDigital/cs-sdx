using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Configuration;

namespace Sdx.Html
{
  public class ImageUploader : FormElement
  {
    private Html.Tag wrapper;
    private Html.Tag listHtml;
    private Html.Tag buttonLabel;
    private Html.VoidTag inputFile;
    private Html.Tag serverImageWrapper;

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
      HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
      //ヘッダー長を引く。IISの最大ヘッダー長は16KB
      //http://stackoverflow.com/questions/686217/maximum-on-http-header-values
      MaxRequestLength = (section.MaxRequestLength - 16 * 1024).ToString();

      MaxCount = 1;
      MaxCountMessage = Sdx.I18n.GetString("%MaxCount%枚まで登録可能です。以下の画像はアップロードされませんでした。");
      MaxRequestLengthMessage = Sdx.I18n.GetString("一度に{0}KB以上はアップロードできません。", MaxRequestLength);
      UnknownErrorMessage = Sdx.I18n.GetString("サーバーでエラーが発生しました。しばらくしてからもう一度お試しください。");
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

      var countErrors = new Sdx.Html.Tag("ul", "sdx-has-error", "count-errors");
      wrapper.AddHtml(countErrors);

      var errors = new Sdx.Html.Tag("ul", "sdx-has-error", "errors");
      wrapper.AddHtml(errors);

      serverImageWrapper = new Html.Tag("div");
      wrapper.AddHtml(serverImageWrapper);

      return wrapper;
    }

    protected internal override void BindValueToTag()
    {
      serverImageWrapper.Children.RemoveAll(tag => true);
      foreach (var val in Value)
      {
        if(val == "" || val == null)
        {
          continue;
        }
        var hidden = new Sdx.Html.InputHidden();
        hidden.Tag.Attr["value"] = val;
        hidden.Tag.Attr.AddClass("server-images");
        serverImageWrapper.AddHtml(hidden.Tag);
      }
    }

    public HtmlBase ButtonLabel
    {
      set
      {
        buttonLabel.Children.RemoveRange(0, buttonLabel.Children.Count);
        buttonLabel.AddChild(value);
      }

      get
      {
        return buttonLabel.Children[0];
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

    public int ThumbHeight
    {
      get
      {
        return Int32.Parse(wrapper.Attr["data-thumb-height"]);
      }
      set
      {
        wrapper.Attr["data-thumb-height"] = value.ToString();
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

    public string MaxRequestLength
    {
      set
      {
        wrapper.Attr["data-max-request-length"] = value;
      }

      get
      {
        return wrapper.Attr["data-max-request-length"];
      }
    }

    public string MaxRequestLengthMessage
    {
      set
      {
        wrapper.Attr["data-max-request-length-message"] = value;
      }

      get
      {
        return wrapper.Attr["data-max-request-length-message"];
      }
    }

    public string UnknownErrorMessage
    {
      set
      {
        wrapper.Attr["data-unknown-error-message"] = value;
      }

      get
      {
        return wrapper.Attr["data-unknown-error-message"];
      }
    }
  }
}
