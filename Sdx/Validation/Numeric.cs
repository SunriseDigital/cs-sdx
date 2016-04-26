using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class Numeric:Validator
  {
    public const string ErrorNotNumeric = "ErrorNotNumeric";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorNotNumeric:
          return Sdx.I18n.GetString("数字を入力してください。");
        default:
          return null;
      }
    }

    protected override bool IsValidString(string value)
    {
      double result;
      if (!double.TryParse(value, out result))
      {
        this.AddError(ErrorNotNumeric);
        return false;
      }

      return true;
    }
  }
}