using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Type : Validator
  {
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

    public List<Sdx.Image.Format> FormatList = new List<Sdx.Image.Format>();

    public Type(Sdx.Image.Format jpeg = Sdx.Image.Format.NONE, Sdx.Image.Format png = Sdx.Image.Format.NONE, Sdx.Image.Format gif = Sdx.Image.Format.NONE)
    {
      if (jpeg == Sdx.Image.Format.NONE && png == Sdx.Image.Format.NONE && gif == Sdx.Image.Format.NONE)
      {
        throw new ArgumentNullException("jpeg,png,gif", "jpeg and png and gif are both null.");
      }

      if(jpeg != Sdx.Image.Format.NONE){
        if(jpeg != Sdx.Image.Format.JPEG){
          throw new Exception("The argument value of jpeg is different");
        }
        this.FormatList.Add(Sdx.Image.Format.JPEG);
      }

      if(png != Sdx.Image.Format.NONE){
        if(png != Sdx.Image.Format.PNG){
          throw new Exception("The argument value of png is different");
        }
        this.FormatList.Add(Sdx.Image.Format.PNG);
      }

      if(gif != Sdx.Image.Format.NONE){
        if(gif != Sdx.Image.Format.GIF){
          throw new Exception("The argument value of gif is different");
        }
        this.FormatList.Add(Sdx.Image.Format.GIF);
      }
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      foreach(var format in this.FormatList){
        if(format == value.Type){
          return true;
        }
      }

      this.AddError(ErrorOtherThanTargetFormat);
      return false;
    }
  }
}
