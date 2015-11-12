using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Validation
{
  public class Whitelist : Validator
  {
    public const string ErrorNotIn = "ErrorNotIn";

    public IEnumerable<string> List { get; private set; }

    public Whitelist(IEnumerable<string> whitelist)
    {
      this.List = whitelist;
    }

    protected override bool IsValidString(string value)
    {
      if (!this.List.Any(val => val == value))
      {
        this.AddError(ErrorNotIn);
        return false;
      }

      return true;
    }
  }
}