using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;
using System.Reflection;
using System.IO;

namespace Sdx.Validation
{
  public abstract class Base
  {
    public Errors Errors { get; internal set; }

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

    public Func<string, Base, string> MessageDetector { get; set; }

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
  }
}
