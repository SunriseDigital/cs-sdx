using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class Regex: Validator
  {
    public const string ErrorNotMatch = "ErrorNotMatch";

    protected override void InitDefaultMessages(Dictionary<string, string> defaultMessages)
    {
      defaultMessages[ErrorNotMatch] = Sdx.I18n.GetString("書式が正しくありません。");
    }

    public System.Text.RegularExpressions.Regex Pattern { get; set; }

    public Regex(string pattern, string message = null):base(message)
    {
      this.Pattern = new System.Text.RegularExpressions.Regex(pattern);
    }

    protected override bool IsValidString(string value)
    {
      if (!this.Pattern.IsMatch(value))
      {
        this.AddError(ErrorNotMatch);
        return false;
      }

      return true;
    }
  }
}