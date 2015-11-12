using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class InputDate : Input
  {
    public InputDate():base()
    {

    }

    public InputDate(string name):base(name)
    {

    }

    protected internal override string GetInputType()
    {
      return "date";
    }
  }
}
