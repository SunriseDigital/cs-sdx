using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class MaxSize : Validator
  {
    public const string ErrorOverWidhtLimit = "ErrorOverWidhtLImit";
    public const string ErrorOverHeightLimit = "ErrorOverHeightLImit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorOverWidhtLimit:
          return Sdx.I18n.GetString("高さが{0}より小さい画像を入力してください。", MaxHeight);
        case ErrorOverHeightLimit:
          return Sdx.I18n.GetString("幅が{0}より小さい画像を入力してください。", MaxWidht);
        default:
          return null;
      }
    }

    public int? MaxHeight { get; set; }
    public int? MaxWidht { get; set; }

    public MaxSize(int? height = null, int? widht = null)
    {
      if (height == null && widht == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.MaxHeight = height;
      this.MaxWidht = widht;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.MaxHeight != null)
      {
        if (value.Height >= this.MaxHeight)
        {
          this.AddError(ErrorOverWidhtLimit);
        }
      }

      if(this.MaxWidht != null)
      {
        if (value.Width >= this.MaxWidht)
        {
          this.AddError(ErrorOverHeightLimit);
        }
      }

      if(this.Errors.Count() > 0){
        return false;
      }

      return true;
    }
  }
}
