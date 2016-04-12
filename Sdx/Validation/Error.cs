using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sdx.Validation
{
  public class Error
  {
    public string ErrorType { get; internal set; }
    public string ClassName { get; internal set; }
    public string Message { get; internal set; }

    public override string ToString()
    {
      return Message;
    }
  }
}