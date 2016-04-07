using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class Date : Validator
  {
    public const string ErrorNotDate = "ErrorNotDate";

    protected override void InitDefaultMessages(Dictionary<string, string> defaultMessages)
    {
      defaultMessages[ErrorNotDate] = I18n.GetString("日付を入力してください。");
    }

    public Date(string message = null)
      : base(message)
    {
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
