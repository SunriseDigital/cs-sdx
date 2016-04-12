using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Validation
{
  public class Whitelist : Validator
  {
    public const string ErrorNotIn = "ErrorNotIn";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorNotIn:
          return Sdx.I18n.GetString("不正な値です。");
        default:
          return null;
      }
    }

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