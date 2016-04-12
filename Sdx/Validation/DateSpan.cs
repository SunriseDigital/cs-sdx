using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class DateSpan : Validator
  {
    public const string ErrorInvalid = "ErrorInvalid";
    public const string ErrorIsEarlier = "ErrorIsEarlier";
    public const string ErrorIsLater = "ErrorIsLater";


    protected override string GetDefaultMessage(string errorType)
    {
      switch (errorType)
      {
        case ErrorInvalid:
          return Sdx.I18n.GetString("日付を入力してください。");
        case ErrorIsEarlier:
          return Sdx.I18n.GetString("{0}以降の日付を入力してください。", ((System.DateTime)Min).ToString(DateFormat));
        case ErrorIsLater:
          return Sdx.I18n.GetString("{0}以前の日付を入力してください。", ((System.DateTime)Max).ToString(DateFormat));
        default:
          return null;
      }
    }


    private System.DateTime? minDate;
    private System.DateTime? maxDate;

    public System.DateTime? Min
    {
      get
      {
        return this.minDate;
      }

      set
      {
        this.minDate = this.TruncateTime(value);
      }
    }

    public System.DateTime? Max
    {
      get
      {
        return this.maxDate;
      }

      set
      {
        this.maxDate = this.TruncateTime(value);
      }
    }

    public string DateFormat { get; private set; }

    private System.DateTime? TruncateTime(System.DateTime? dateTime)
    {
      if(dateTime != null)
      {
        dateTime = ((System.DateTime)dateTime).Date;
      }

      return dateTime;
    }

    public DateSpan(System.DateTime? min = null, System.DateTime? max = null, string dateFormat = null)
    {
      if (min == null && max == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.Min = min;
      this.Max = max;
      if (dateFormat == null)
      {
        dateFormat = "d";
      }
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

      targetDate = targetDate.Date;

      if(this.Min != null)
      {
        var min = (System.DateTime)this.Min;
        if (min.CompareTo(targetDate) > 0)
        {
          this.AddError(ErrorIsEarlier);
          return false;
        }
      }

      if(this.Max != null)
      {
        var max = (System.DateTime)this.Max;
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
