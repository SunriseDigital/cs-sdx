﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class DateTime : Validator
  {
    public const string ErrorNotDateTime = "ErrorNotDateTime";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorNotDateTime:
          return Sdx.I18n.GetString("日時を入力してください。");
        default:
          return null;
      }
    }

    protected override bool IsValidString(string value)
    {
      System.DateTime result;
      if (!System.DateTime.TryParse(value, out result))
      {
        this.AddError(ErrorNotDateTime);
        return false;
      }

      var reg = new System.Text.RegularExpressions.Regex(":[0-9]{2}$");
      if (!reg.Match(value).Success)
      {
        this.AddError(ErrorNotDateTime);
        return false;
      }

      return true;
    }
  }
}
