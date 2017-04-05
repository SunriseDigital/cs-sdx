using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class NotEmpty : Validator
  {
    public const string ErrorNotEmpty = "ErrorNotEmpty";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorNotEmpty:
          return Sdx.I18n.GetString("必須項目です。");
        default:
          return null;
      }
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if (value == null)
      {
        AddError(ErrorNotEmpty);
        return false;
      }

      return true;
    }
  }
}
