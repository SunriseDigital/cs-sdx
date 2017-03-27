using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Capacity : Validator
  {
    public const string ErrorOverCapacityLimit = "ErrorOverCapacityLImit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorOverCapacityLimit:
          return Sdx.I18n.GetString("{0}より小さいサイズの画像が登録可能です。", Sdx.Util.Number.BytesToHumanRedable(MaxCapacity));
        default:
          return null;
      }
    }

    public int MaxCapacity { get; set; }

    public Capacity(int bytes)
    {
      this.MaxCapacity = bytes;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if (value.Size >= this.MaxCapacity)
      {
        this.AddError(ErrorOverCapacityLimit);
        return false;
      }

      return true;
    }
  }
}
