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

    protected override void InitDefaultMessages(Dictionary<string, string> defaultMessages)
    {
      defaultMessages[ErrorTooShort] = I18n.GetString("%min%文字以上入力してください（現在%actual_length%文字）。");
      defaultMessages[ErrorTooLong] = I18n.GetString("%max%文字までしか入力できません（現在%actual_length%文字）。");
    }

    private long? min;
    private long? max;

    public long? Min
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

    public long? Max
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

    public StringLength(long? min = null, long? max = null, string message = null)
      : base(message)
    {
      if(min == null && max == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }
      this.Min = min;
      this.Max = max;
    }


    protected override bool IsValidString(string value)
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
