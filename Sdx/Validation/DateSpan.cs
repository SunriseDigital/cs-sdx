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

    private DateTime? minDate;
    private DateTime? maxDate;

    public DateTime? Min
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

    public DateTime? Max
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

    private DateTime? TruncateTime(DateTime? dateTime)
    {
      if(dateTime != null)
      {
        dateTime = ((DateTime)dateTime).Date;
      }

      return dateTime;
    }

    public DateSpan(DateTime? min = null, DateTime? max = null, string dateFormat = null, string message = null): base(message)
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
      DateTime targetDate;
      if (!DateTime.TryParse(value, out targetDate))
      {
        this.AddError(ErrorInvalid);
        return false;
      }

      targetDate = targetDate.Date;

      if(this.Min != null)
      {
        var min = (DateTime)this.Min;
        this.SetPlaceholder("min", min.ToString(this.DateFormat));
        if (min.CompareTo(targetDate) > 0)
        {
          this.AddError(ErrorIsEarlier);
          return false;
        }
      }

      if(this.Max != null)
      {
        var max = (DateTime)this.Max;
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
