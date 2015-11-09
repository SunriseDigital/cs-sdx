using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class NotEmpty : Validator
  {
    public const string ErrorIsEmpty = "ErrorIsEmpty";

    public NotEmpty(string message = null) : base(message)
    {
    }

    public NotEmpty(Dictionary<string, string> messages) : base(messages)
    {
    }

    protected override bool ExecIsValue(IEnumerable<string> values)
    {
      if (values == null)
      {
        this.AddError(ErrorIsEmpty);
        return false;
      }

      if(values.Count() == 0)
      {
        this.AddError(ErrorIsEmpty);
        return false;
      }

      return true;
    }

    protected override bool ExecIsValue(string value)
    {
      if (value == null)
      {
        this.AddError(ErrorIsEmpty);
        return false;
      }

      if(value == "")
      {
        this.AddError(ErrorIsEmpty);
        return false;
      }

      return true;
    }
  }
}
