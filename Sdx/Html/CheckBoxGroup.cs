using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sdx.Html
{
  public class CheckBoxGroup : CheckableGroup
  {
    protected internal override FormValue CreateFormValue()
    {
      return new FormValue(true);
    }

    protected internal override Checkable CreateCheckable()
    {
      return new CheckBox();
    }

    public CheckBoxGroup():base()
    {

    }

    public CheckBoxGroup(string name)
      : base(name)
    {

    }
  }
}
