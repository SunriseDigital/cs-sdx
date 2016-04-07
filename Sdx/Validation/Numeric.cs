using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class Numeric:Validator
  {
    public const string ErrorNotNumeric = "ErrorNotNumeric";

    protected override void InitDefaultMessages(Dictionary<string, string> defaultMessages)
    {
      defaultMessages[ErrorNotNumeric] = I18n.GetString("数字を入力してください。");
    }

    public Numeric(string message = null) : base(message)
    {
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