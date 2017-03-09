using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class MinSize : Validator
  {
    public const string ErrorUnderWidhtLImit = "ErrorUnderWidhtLImit";
    public const string ErrorUnderHeightLImit = "ErrorUnderHeightLImit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorUnderWidhtLImit:
          return Sdx.I18n.GetString("高さが{0}より大きい画像を入力してください。", MinHeight);
        case ErrorUnderHeightLImit:
          return Sdx.I18n.GetString("幅が{0}より大きい画像を入力してください。", MinWidht);
        default:
          return null;
      }
    }

    public int? MinHeight { get; set; }
    public int? MinWidht { get; set; }

    public MinSize(int? height = null, int? widht = null)
    {
      if (height == null && widht == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.MinHeight = height;
      this.MinWidht = widht;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.MinHeight != null)
      {
        if (value.Height <= this.MinHeight)
        {
          this.AddError(ErrorUnderWidhtLImit);
        }
      }

      if(this.MinWidht != null)
      {
        if (value.Width <= this.MinWidht)
        {
          this.AddError(ErrorUnderHeightLImit);
        }
      }

      if(this.Errors.Count() > 0){
        return false;
      }
      
      return true;
    }
  }
}
