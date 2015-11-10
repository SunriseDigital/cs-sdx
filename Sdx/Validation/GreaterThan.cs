using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class GreaterThan : Validator
  {
    public const string ErrorInvalid = "ErrorInvalid";
    public const string ErrorGreaterThanInclusive = "ErrorGreaterThanInclusive";
    public const string ErrorGreaterThan = "ErrorGreaterThan";

    private long min;

    public long Min
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

    public bool Inclusive { get; set; } = false;


    public GreaterThan(long min, string message = null) : base(message)
    {
      this.Min = min;
    }

    public GreaterThan(long min, bool inclusive, string message = null) : base(message)
    {
      this.Min = min;
      this.Inclusive = inclusive;
    }

    protected override bool ExecIsValue(string value)
    {
      long intValue = 0;
      if (!Int64.TryParse(value, out intValue))
      {
        this.AddError(ErrorInvalid);
        return false;
      }

      if (this.Inclusive)
      {
        if (intValue >= this.Min)
        {
          return true;
        }

        this.AddError(ErrorGreaterThanInclusive);
      }
      else
      {
        if (intValue > this.Min)
        {
          return true;
        }

        this.AddError(ErrorGreaterThan);
      }

      return false;
    }
  }
}