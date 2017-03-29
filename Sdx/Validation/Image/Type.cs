using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Type : Validator
  {
    public const string ErrorOtherThanTargetFormat = "ErrorOtherThanTargetFormat";

    protected override string GetDefaultMessage(string errorType)
    {
      var FormaStringtList = FormatList.Select(c => c.ToString());
      var messageValue = string.Join(",", FormaStringtList);

      switch (errorType)
      {
        case ErrorOtherThanTargetFormat:
          return Sdx.I18n.GetString("「{0}」が登録可能です。", messageValue);
        default:
          return null;
      }
    }

    public List<Sdx.Image.Format> FormatList = new List<Sdx.Image.Format>();

    public Type(params Sdx.Image.Format[] types)
    {
      this.FormatList.AddRange(types);
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      foreach(var format in this.FormatList){
        if(format == value.Type){
          return true;
        }
      }

      this.AddError(ErrorOtherThanTargetFormat);
      return false;
    }
  }
}
