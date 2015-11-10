using System;

namespace Sdx.Validation
{
  public class LessThan : Validator
  {
    public const string ErrorInvalid = "ErrorInvalid";
    public const string ErrorLessEqual = "ErrorLessEqual";
    public const string ErrorLessThan = "ErrorLessThan";

    private int max;

    public int Max
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

    public bool Inclusive { get; set; } = false;

    public LessThan(int max):base()
    {
      this.Max = max;
    }

    public LessThan(int max, bool inclusive):base()
    {
      this.Max = max;
      this.Inclusive = inclusive;
    }

    public LessThan(int max, string message) : base(message)
    {
      this.Max = max;
    }

    public LessThan(int max, bool inclusive, string message) : base(message)
    {
      this.Max = max;
      this.Inclusive = inclusive;
    }

    protected override bool ExecIsValue(string value)
    {
      int intValue = 0;
      if (!Int32.TryParse(value, out intValue))
      {
        this.AddError(ErrorInvalid);
        return false;
      }

      if (this.Inclusive)
      {
        if (intValue <= this.Max)
        {
          return true;
        }

        this.AddError(ErrorLessEqual);
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