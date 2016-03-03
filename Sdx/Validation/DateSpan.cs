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

    public DateSpan(System.DateTime? min = null, System.DateTime? max = null, string dateFormat = null, string message = null)
      : base(message)
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
