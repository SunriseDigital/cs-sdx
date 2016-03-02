using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class Date : Validator
  {
    public const string ErrorNotDate = "ErrorNotDate";

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
