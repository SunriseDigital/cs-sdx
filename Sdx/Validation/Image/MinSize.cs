﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class MinSize : Validator
  {
    public const string ErrorUnderWidthLimit = "ErrorUnderWidthLimit";
    public const string ErrorUnderHeightLimit = "ErrorUnderHeightLimit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorUnderWidthLimit:
          return Sdx.I18n.GetString("高さが{0}px以上の画像が登録可能です。", MinHeight);
        case ErrorUnderHeightLimit:
          return Sdx.I18n.GetString("幅が{0}px以上の画像が登録可能です。", MinWidth);
        default:
          return null;
      }
    }

    public int? MinHeight { get; set; }
    public int? MinWidth { get; set; }

    public MinSize(int? width = null, int? height = null)
    {
      if (height == null && width == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.MinHeight = height;
      this.MinWidth = width;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if (this.MinWidth != null)
      {
        if (value.Width < this.MinWidth)
        {
          this.AddError(ErrorUnderHeightLimit);
        }
      }

      if(this.MinHeight != null)
      {
        if (value.Height < this.MinHeight)
        {
          this.AddError(ErrorUnderWidthLimit);
        }
      }

      if(this.Errors.Count() > 0){
        return false;
      }
      
      return true;
    }
  }
}
