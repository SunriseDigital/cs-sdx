using System;
using System.Collections.Generic;

namespace Sdx.Validation
{
  public class LessThan : Validator
  {
    public const string ErrorInvalid = "ErrorInvalid";
    public const string ErrorLessThanInclusive = "ErrorLessThanInclusive";
    public const string ErrorLessThan = "ErrorLessThan";

    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorInvalid:
          return Sdx.I18n.GetString("数字を入力してください。");
        case ErrorLessThanInclusive:
          return Sdx.I18n.GetString("{0}以下の数字を入力してください。", Max);
        case ErrorLessThan:
          return Sdx.I18n.GetString("{0}未満の数字を入力してください。", Max);
        default:
          return null;
      }
    }

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
      }
    }

    public bool IsInclusive { get; set; }

    public LessThan(long max)
    {
      this.Max = max;
      this.IsInclusive = false;
    }

    public LessThan(long max, bool isInclusive)
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