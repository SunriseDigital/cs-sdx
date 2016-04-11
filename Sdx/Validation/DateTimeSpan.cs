using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class DateTimeSpan : Validator
  {
    public const string ErrorInvalid = "ErrorInvalid";
    public const string ErrorIsEarlier = "ErrorIsEarlier";
    public const string ErrorIsLater = "ErrorIsLater";

    protected override void InitDefaultMessages(Dictionary<string, string> defaultMessages)
    {
      defaultMessages[ErrorInvalid] = Sdx.I18n.GetString("日時を入力してください。");
      defaultMessages[ErrorIsEarlier] = Sdx.I18n.GetString("%min%以降の日時を入力してください。");
      defaultMessages[ErrorIsLater] = Sdx.I18n.GetString("%max%以前の日時を入力してください。");
    }

    public System.DateTime? Min { get; set; }
    public System.DateTime? Max { get; set; }

    public string DateFormat { get; private set; }

    public DateTimeSpan(System.DateTime? min = null, System.DateTime? max = null, string dateFormat = null, string message = null)
      : base(message)
    {
      if (min == null && max == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.Min = min;
      this.Max = max;
      this.DateFormat = dateFormat;
    }

    protected override bool IsValidString(string value)
    {
      System.DateTime targetDate;
      if (!System.DateTime.TryParse(value, out targetDate))
      {
        this.AddError(ErrorInvalid);
        return false;
      }

      if(this.Min != null)
      {
        var min = (System.DateTime)this.Min;
        this.SetPlaceholder("min", min.ToString(this.DateFormat));
        if (min.CompareTo(targetDate) > 0)
        {
          this.AddError(ErrorIsEarlier);
          return false;
        }
      }

      if(this.Max != null)
      {
        var max = (System.DateTime)this.Max;
        this.SetPlaceholder("max", max.ToString(this.DateFormat));
        if (max.CompareTo(targetDate) < 0)
        {
          this.AddError(ErrorIsLater);
          return false;
        }
      }

      
      return true;
    }
  }
}
