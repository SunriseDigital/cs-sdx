using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sdx.Validation.Image
{
  public class Capacity : Validator
  {
    public const string ErrorOverCapacityLImit = "ErrorOverCapacityLImit";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorOverCapacityLImit:
          return Sdx.I18n.GetString("{0}バイトより小さいサイズの画像を入力してください。", MaxCapacity);
        default:
          return null;
      }
    }

    public int MaxCapacity { get; set; }

    public Capacity(int capacity)
    {
      this.MaxCapacity = capacity;
    }

    protected override bool IsValidImage(Sdx.Image value)
    {
      if(this.MaxCapacity != null)
      {
        if (value.Size >= this.MaxCapacity)
        {
          this.AddError(ErrorOverCapacityLImit);
          return false;
        }
      }

      return true;
    }
  }
}
