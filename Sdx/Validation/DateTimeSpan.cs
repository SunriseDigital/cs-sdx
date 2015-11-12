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

    public DateTime? Min { get; set; }
    public DateTime? Max { get; set; }

    public string DateFormat { get; private set; }

    public DateTimeSpan(DateTime? min = null, DateTime? max = null, string dateFormat = null, string message = null): base(message)
    {
      if (min == null && max == null)
      {
        throw new ArgumentNullException("min and max are both null.");
      }

      this.Min = min;
      this.Max = max;
      this.DateFormat = dateFormat;
    }

    protected override bool ExecIsValid(string value)
    {
      DateTime targetDate;
      if (!DateTime.TryParse(value, out targetDate))
      {
        this.AddError(ErrorInvalid);
        return false;
      }

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
