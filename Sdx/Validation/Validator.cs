using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;

namespace Sdx.Validation
{
  public abstract class Validator
  {
    internal Errors Errors { get; set; }

    protected abstract bool Execute(object value);

    public bool IsValid(object value)
    {
      return Execute(value);
    }
  }
}
