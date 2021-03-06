﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class MaxCapacity : Validator
  {
    public const string ErrorOverCapacityLimit = "ErrorOverCapacityLImit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorOverCapacityLimit:
          return Sdx.I18n.GetString("{0}より小さいサイズの画像が登録可能です。", Sdx.Util.Number.BytesToHumanRedable(BytesValue));
        default:
          return null;
      }
    }

    public int BytesValue { get; set; }

    public MaxCapacity(int bytes)
    {
      this.BytesValue = bytes;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if (value.Size >= this.BytesValue)
      {
        this.AddError(ErrorOverCapacityLimit);
        return false;
      }

      return true;
    }
  }
}
