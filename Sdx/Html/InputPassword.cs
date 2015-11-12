using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class InputPassword : Input
  {

    public InputPassword():base()
    {

    }

    public InputPassword(string name):base(name)
    {

    }
    
    protected internal override string GetInputType()
    {
      return "password";
    }
  }
}
