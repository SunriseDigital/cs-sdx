using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class MaxSize : Validator
  {
    public const string ErrorOverWidhtLImit = "ErrorOverWidhtLImit";
    public const string ErrorOverHeightLImit = "ErrorOverHeightLImit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorOverWidhtLImit:
          return Sdx.I18n.GetString("高さが{0}より小さい画像を入力してください。", MaxHeight);
        case ErrorOverHeightLImit:
          return Sdx.I18n.GetString("幅が{0}より小さい画像を入力してください。", MaxWidht);
        default:
          return null;
      }
    }

    public int? MaxHeight { get; set; }
    public int? MaxWidht { get; set; }

    public string DateFormat { get; private set; }

    public MaxSize(int? maxHeight = null, int? maxWidht = null)
    {
      if (maxHeight == null && maxWidht == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.MaxHeight = maxHeight;
      this.MaxWidht = maxWidht;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.MaxHeight != null)
      {
        if (value.Height >= this.MaxHeight)
        {
          this.AddError(ErrorOverWidhtLImit);
        }
      }

      if(this.MaxWidht != null)
      {
        if (value.Width >= this.MaxWidht)
        {
          this.AddError(ErrorOverHeightLImit);
        }
      }

      if(this.Errors.Count() > 0){
        return false;
      }
      
      return true;
    }
  }
}
