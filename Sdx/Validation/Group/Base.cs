using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation.Group
{
  public abstract class Base
  {
    public Errors Errors { get; private set; }

    public Base()
    {
      Errors = new Errors();
    }

    public bool Exec(Html.Form form)
    {
      Errors.Clear();
      return IsValidForm(form);
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

    protected abstract bool IsValidForm(Html.Form form);
  }
}
