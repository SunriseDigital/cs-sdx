using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class Numeric:Validator
  {
    public const string ErrorNotNumeric = "ErrorNotNumeric";

    public Numeric(string message = null) : base(message)
    {
    }

    public Numeric(Dictionary<string, string> messages) : base(messages)
    {
    }

    protected override bool ExecIsValue(string value)
    {
      long result;
      if (!Int64.TryParse(value, out result))
      {
        this.AddError(ErrorNotNumeric);
        return false;
      }

      return true;
    }
  }
}