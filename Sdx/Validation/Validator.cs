using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;
using System.Reflection;
using System.IO;

namespace Sdx.Validation
{
  public abstract class Validator : Base.Validator
  {
    protected abstract bool IsValidString(string value);

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
