using System;
using System.Collections.Generic;
using System.Linq;

namespace Sdx.Validation
{
  public class Whitelist : Validator
  {
    public const string ErrorNotIn = "ErrorNotIn";

    protected override void InitDefaultMessages(Dictionary<string, string> defaultMessages)
    {
      defaultMessages[ErrorNotIn] = I18n.GetString("不正な値です。");
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