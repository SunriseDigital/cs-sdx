using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sdx.Validation
{
  public class Error
  {
    public string ErrorType { get; private set; }
    public string ClassName { get; internal set; }
    public string Message { get; set; }

    public Error()
    {

    }

    public Error(string errorType):this()
    {
      ErrorType = errorType;
    }

    public override string ToString()
    {
      return Message;
    }
  }
}