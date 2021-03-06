﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class MaxSize : Validator
  {
    public const string ErrorOverWidthLimit = "ErrorOverWidthLimit";
    public const string ErrorOverHeightLimit = "ErrorOverHeightLimit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorOverWidthLimit:
          return Sdx.I18n.GetString("高さが{0}px以下の画像が登録可能です。", MaxHeight);
        case ErrorOverHeightLimit:
          return Sdx.I18n.GetString("幅が{0}px以下の画像が登録可能です。", MaxWidth);
        default:
          return null;
      }
    }

    public int? MaxHeight { get; set; }
    public int? MaxWidth { get; set; }

    public MaxSize(int? width = null, int? height = null)
    {
      if (height == null && width == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.MaxHeight = height;
      this.MaxWidth = width;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.MaxWidth != null)
      {
        if (value.Width > this.MaxWidth)
        {
          this.AddError(ErrorOverHeightLimit);
        }
      }

      if (this.MaxHeight != null)
      {
        if (value.Height > this.MaxHeight)
        {
          this.AddError(ErrorOverWidthLimit);
        }
      }

      if(this.Errors.Count() > 0){
        return false;
      }

      return true;
    }
  }
}
