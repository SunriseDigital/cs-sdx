using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;

namespace Sdx.Validation
{
  public abstract class Validator
  {
    public Errors Errors { get; internal set; }

    protected abstract bool ExecIsValue(string value);

    protected virtual bool ExecIsValue(IEnumerable<string> values)
    {
      var result = true;
      foreach (var value in values)
      {
        if (!ExecIsValue(value))
        {
          result = false;
          break;
        }
      }

      return result;
    }

    protected void AddError(string errorType)
    {
      var error = new Error(this.GetType().FullName, errorType, Sdx.Context.Current.Lang);
      Errors.Add(error);
    }

    protected Dictionary<string, string> MessagePlaceholder = new Dictionary<string, string>();


    public bool IsValid(IEnumerable<string> values)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }
      return ExecIsValue(values);
    }

    public bool IsValid(string value)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }
      return ExecIsValue(value);
    }
  }
}
