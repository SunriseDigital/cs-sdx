using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class NotEmpty : Validator
  {
    private const string ErrorIsEmpty = "ErrorIsEmpty";

    protected static readonly Dictionary<string, Dictionary<string, string>> DefaultMessages = new Dictionary<string, Dictionary<string, string>>
    {
      {
        "ja", new Dictionary<string, string>
        {
          {"ErrorIsEmpty", "必須項目です。"}
        }
      }
    };

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
