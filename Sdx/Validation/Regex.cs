using System;

namespace Sdx.Validation
{
  public class Regex: Validator
  {
    public const string ErrorNotMatch = "ErrorNotMatch";

    public System.Text.RegularExpressions.Regex Pattern { get; set; }

    public Regex(string pattern)
    {
      this.Pattern = new System.Text.RegularExpressions.Regex(pattern);
    }

    protected override bool ExecIsValue(string value)
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