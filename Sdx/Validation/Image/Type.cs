using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Type : Validator
  {
    public const string Jpeg = "JPEG";
    public const string png = "PNG";
    public const string gif = "GIF";
    public const string ErrorOtherThanTargetFormat = "ErrorOtherThanTargetFormat";

    protected override string GetDefaultMessage(string errorType)
    {
      var FormaStringtList = FormatList.Select(c => c.ToString());
      var messageValue = string.Join(",", FormaStringtList);

      switch (errorType)
      {
        case ErrorOtherThanTargetFormat:
          return Sdx.I18n.GetString("拡張子が「{0}」の画像を入力してください。", messageValue);
        default:
          return null;
      }
    }

    public List<Sdx.Image.FileType> FormatList = new List<Sdx.Image.FileType>();

    public Type(Sdx.Image.FileType? jpeg = null, Sdx.Image.FileType? png = null, Sdx.Image.FileType? gif = null)
    {
      if (jpeg == null && png == null && gif == null)
      {
        throw new ArgumentNullException("jpeg and png and gif are both null.");
      }

      if(jpeg != null){
        if(jpeg != Sdx.Image.FileType.JPEG){
          throw new Exception("The argument value of jpeg is different");
        }
        this.FormatList.Add(Sdx.Image.FileType.JPEG);
      }

      if(png != null){
        if(png != Sdx.Image.FileType.PNG){
          throw new Exception("The argument value of png is different");
        }
        this.FormatList.Add(Sdx.Image.FileType.PNG);
      }

      if(gif != null){
        if(gif != Sdx.Image.FileType.GIF){
          throw new Exception("The argument value of gif is different");
        }
        this.FormatList.Add(Sdx.Image.FileType.GIF);
      }

    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.FormatList.IndexOf(Sdx.Image.FileType.JPEG) > -1)
      {
        if (value.Type != Sdx.Image.FileType.JPEG)
        {
          this.AddError(ErrorOtherThanTargetFormat);
          return false;
        }
      }

      if(this.FormatList.IndexOf(Sdx.Image.FileType.PNG) > -1)
      {
        if (value.Type != Sdx.Image.FileType.PNG)
        {
          this.AddError(ErrorOtherThanTargetFormat);
          return false;
        }
      }

      if(this.FormatList.IndexOf(Sdx.Image.FileType.GIF) > -1)
      {
        if (value.Type != Sdx.Image.FileType.GIF)
        {
          this.AddError(ErrorOtherThanTargetFormat);
          return false;
        }
      }

      return true;
    }
  }
}
