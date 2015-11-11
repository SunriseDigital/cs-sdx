using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class StringLength : Validator
  {
    public const string ErrorTooShort = "ErrorTooShort";
    public const string ErrorTooLong = "ErrorTooLong";

    private int? min;
    private int? max;

    public int? Min
    {
      get
      {
        return this.min;
      }

      set
      {
        this.min = value;
        this.SetPlaceholder("min", value.ToString());
      }
    }

    public int? Max
    {
      get
      {
        return this.max;
      }

      set
      {
        this.max = value;
        this.SetPlaceholder("max", this.max.ToString());
      }
    }

    public StringLength(int? min = null, int? max = null, string message = null) : base(message)
    {
      this.Min = min;
      this.Max = max;
    }


    protected override bool ExecIsValid(string value)
    {
      int length = value.Length;
      this.SetPlaceholder("actual_length", length.ToString());

      if (this.Min != null && length < this.Min)
      {
        this.AddError(ErrorTooShort);
        return false;
      }

      if (this.Max != null && this.Max < length)
      {
        this.AddError(ErrorTooLong);
        return false;
      }

      return true;
    }
  }
}
