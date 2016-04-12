using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class Date : Validator
  {
    public const string ErrorNotDate = "ErrorNotDate";


    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorNotDate:
          return Sdx.I18n.GetString("日付を入力してください。");
        default:
          return null;
      }
    }

    protected override bool IsValidString(string value)
    {
      System.DateTime result;
      if (!System.DateTime.TryParse(value + " 00:00:00", out result))
      {
        this.AddError(ErrorNotDate);
        return false;
      }

      return true;
    }
  }
}
