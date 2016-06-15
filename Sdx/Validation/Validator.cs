using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;
using System.Reflection;
using System.IO;

namespace Sdx.Validation
{
  public abstract class Validator
  {
    public Errors Errors { get; internal set; }

    protected abstract bool IsValidString(string value);

    protected abstract string GetDefaultMessage(string errorType);

    private string DetectMessage(Error error)
    {
      if(MessageDetector != null)
      {
        return MessageDetector(error.ErrorType, this);
      }

      var message = GetDefaultMessage(error.ErrorType);
      if (message == null)
      {
        throw new NotImplementedException("Missing default message for error " + error.ErrorType);
      }

      return message;
    }

    public Func<string, Validator, string> MessageDetector { get; set; }

    protected void AddError(string errorType)
    {
      var error = new Error(errorType);

      error.ClassName = this.GetType().FullName;

      error.Message = this.DetectMessage(error);

      Errors.Add(error);
    }

    protected void AddError(Error error)
    {
      if (error.Message == null)
      {
        throw new InvalidOperationException("Missing error message");
      }
      error.ClassName = this.GetType().FullName;

      Errors.Add(error);
    }

    public virtual bool IsValid(IEnumerable<string> values)
    {
      var result = true;
      foreach (var value in values)
      {
        if (!IsValid(value))
        {
          result = false;
          break;
        }
      }

      return result;
    }

    public bool IsValid(string value)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }

      Value = value;
      ValueLength = value != null ? value.Length : 0;

      return IsValidString(value);
    }

    public string Value { get; private set; }
    public long ValueLength { get; private set; }
  }
}
