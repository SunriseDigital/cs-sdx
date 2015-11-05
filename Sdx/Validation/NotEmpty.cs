using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Validation
{
  public class NotEmpty : Validator
  {
    protected override bool Execute(object value)
    {
      return true;
    }
  }
}
