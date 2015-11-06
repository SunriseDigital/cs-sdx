using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sdx.Validation;

namespace Sdx.Validation
{
  public class Errors
  {
    private List<Error> errors = new List<Error>();

    public int Count
    {
      get
      {
        return errors.Count;
      }
    }

    public Error this[int index]
    {
      get
      {
        return this.errors[index];
      }
    }

    internal void Add(Error error)
    {
      this.errors.Add(error);
    }
  }
}
