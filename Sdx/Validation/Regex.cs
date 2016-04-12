using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class Regex: Validator
  {
    public const string ErrorNotMatch = "ErrorNotMatch";

    protected override string GetDefaultMessage(string errorType)
    {
      switch(errorType)
      {
        case ErrorNotMatch:
          return Sdx.I18n.GetString("書式が正しくありません。");
        default:
          return null;
      }
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