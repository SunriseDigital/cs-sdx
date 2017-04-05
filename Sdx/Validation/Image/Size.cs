using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Size : Validator
  {
    public const string ErrorInvalidWidth = "ErrorInvalidWidth";
    public const string ErrorInvalidHeight = "ErrorInvalidHeight";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorInvalidHeight:
          return Sdx.I18n.GetString("高さ{0}pxの画像が登録可能です。", Height);
        case ErrorInvalidWidth:
          return Sdx.I18n.GetString("幅{0}pxの画像が登録可能です。", Width);
        default:
          return null;
      }
    }

    public int? Height { get; set; }
    public int? Width { get; set; }

    public Size(int? height = null, int? width = null)
    {
      if (height == null && width == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.Height = height;
      this.Width = width;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if (this.Height != null)
      {
        if (value.Height != this.Height)
        {
          this.AddError(ErrorInvalidHeight);
        }
      }

      if (this.Width != null)
      {
        if (value.Width != this.Width)
        {
          this.AddError(ErrorInvalidWidth);
        }
      }

      if (this.Errors.Count() > 0)
      {
        return false;
      }

      return true;
    }
  }
}
