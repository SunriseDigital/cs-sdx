using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class LessThan : Validator
  {
    public const string ErrorInvalid = "ErrorInvalid";
    public const string ErrorLessThanInclusive = "ErrorLessThanInclusive";
    public const string ErrorLessThan = "ErrorLessThan";

    private long max;

    public long Max
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

    public bool IsInclusive { get; set; }

    public LessThan(long max, string message = null) : base(message)
    {
      this.Max = max;
      this.IsInclusive = false;
    }

    public LessThan(long max, bool isInclusive, string message = null) : base(message)
    {
      this.Max = max;
      this.IsInclusive = isInclusive;
    }

    protected override bool IsValidString(string value)
    {
      long intValue = 0;
      if (!Int64.TryParse(value, out intValue))
      {
        this.AddError(ErrorInvalid);
        return false;
      }

      if (this.IsInclusive)
      {
        if (intValue <= this.Max)
        {
          return true;
        }

        this.AddError(ErrorLessThanInclusive);
      }
      else
      {
        if (intValue < this.Max)
        {
          return true;
        }

        this.AddError(ErrorLessThan);
      }

      return false;
    }
  }
}