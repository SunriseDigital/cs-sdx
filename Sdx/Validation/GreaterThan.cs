﻿using System;
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

    public bool IsInclusive { get; set; }


    public GreaterThan(long min, string message = null) : base(message)
    {
      this.Min = min;
      this.IsInclusive = false;
    }

    public GreaterThan(long min, bool isInclusive, string message = null) : base(message)
    {
      this.Min = min;
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