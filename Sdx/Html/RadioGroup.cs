using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class RadioGroup: CheckableGroup
  {
    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(false);
    }

    protected internal override Checkable CreateCheckable()
    {
      return new Radio();
    }

    public RadioGroup():base()
    {

    }

    public RadioGroup(string name)
      : base(name)
    {

    }
  }
}
