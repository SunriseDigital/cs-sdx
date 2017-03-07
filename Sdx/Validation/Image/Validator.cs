using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Html;
using System.Reflection;
using System.IO;
using System.Drawing;

namespace Sdx.Validation.Image
{
  public abstract class Validator : Base.Validator
  {
    public Sdx.Image Value { get; private set; }

    protected abstract bool IsValidImage(Sdx.Image value);

    public bool IsValid(Sdx.Image value)
    {
      if (this.Errors == null)
      {
        this.Errors = new Errors();
      }

      Value = value;

      return IsValidImage(value);
    }

  }
}
