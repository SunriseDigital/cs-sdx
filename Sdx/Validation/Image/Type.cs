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

    public List<string> FormatList = new List<string>();

    public Type(string jpeg = null, string png = null, string gif = null)
    {
      if (jpeg == null && png == null && gif == null)
      {
        throw new ArgumentNullException("jpeg and png and gif are both null.");
      }

      if(jpeg != null){
        if(jpeg != Type.Jpeg){
          throw new Exception("The argument value of jpeg is different");
        }
        this.FormatList.Add(Type.Jpeg);
      }

      if(png != null){
        if(png != Type.png){
          throw new Exception("The argument value of png is different");
        }
        this.FormatList.Add(Type.png);
      }

      if(gif != null){
        if(gif != Type.gif){
          throw new Exception("The argument value of gif is different");
        }
        this.FormatList.Add(Type.gif);
      }

    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.FormatList.IndexOf(Type.Jpeg) > -1)
      {
        if (value.GetFileFormat() != Type.Jpeg)
        {
          this.AddError(ErrorOtherThanTargetFormat);
          return false;
        }
      }

      if(this.FormatList.IndexOf(Type.png) > -1)
      {
        if (value.GetFileFormat() != Type.png)
        {
          this.AddError(ErrorOtherThanTargetFormat);
          return false;
        }
      }

      if(this.FormatList.IndexOf(Type.gif) > -1)
      {
        if (value.GetFileFormat() != Type.gif)
        {
          this.AddError(ErrorOtherThanTargetFormat);
          return false;
        }
      }

      return true;
    }
  }
}
