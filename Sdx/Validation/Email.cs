using System;
using System.Text.RegularExpressions;

namespace Sdx.Validation
{
  public class Email : Validator
  {
    public const string ErrorInvalidFormat = "ErrorInvalidFormat";

    private static System.Text.RegularExpressions.Regex domainRegex = new System.Text.RegularExpressions.Regex(@"^([A-Za-z0-9][A-Za-z0-9\-]{0,61}[A-Za-z0-9]\.)+[A-Za-z]+$");

    private static System.Text.RegularExpressions.Regex localPartRegex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z0-9\!#\$%&’\*\+\-/=\?\^_`\{\|\}~\.]{1,64}$");

    public Email(string message = null) : base(message)
    {
    }

    protected override bool IsValidString(string value)
    {
      var parts = value.Split('@');
      if (parts.Length != 2)
      {
        this.AddError(ErrorInvalidFormat);
        return false;
      }

      var localPart = parts[0];
      if (!localPartRegex.IsMatch(localPart))
      {
        this.AddError(ErrorInvalidFormat);
        return false;
      }

      var domain = parts[1];
      if (!domainRegex.IsMatch(domain))
      {
        this.AddError(ErrorInvalidFormat);
        return false;
      }
      
      return true;
    }
  }
}